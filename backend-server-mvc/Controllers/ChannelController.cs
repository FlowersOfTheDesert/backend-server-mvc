using backend_server_mvc.Data;
using backend_server_mvc.Dto.Request;
using backend_server_mvc.Dto.Response;
using backend_server_mvc.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace backend_server_mvc.Controllers
{
    public class Message
    {
        public string Content { get; set; }
        public string MessageId { get; set; }
        public string ChannelId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ChannelController : ControllerBase
    {
        private AppDbContext _context;
        IDeviceTokenAuthService _deviceAuthService;
        IUserSessionAuthService _userAuthService;
        ILogger<ChannelController> _logger;

        private static ConcurrentQueue<Message> _messages = new ConcurrentQueue<Message>();

        //channel id, tcs, channelid (again idk why ask claude)
        private static ConcurrentDictionary<string, (TaskCompletionSource<Message> Tcs, string ChannelId)> _waitingClients =
            new ConcurrentDictionary<string, (TaskCompletionSource<Message>, string)>();

        public ChannelController(AppDbContext context, IDeviceTokenAuthService deviceAuthService, IUserSessionAuthService userAuthService, ILogger<ChannelController> logger)
        {
            _context = context;
            _deviceAuthService = deviceAuthService;
            _userAuthService = userAuthService;
            _logger = logger;
        }

        [HttpPost("listener/connect")]
        public IActionResult CreateSession([FromHeader(Name = "Authorization")] string token)
        {
            _logger.LogInformation("Creating new session with token: {TokenPrefix}...", token.Substring(0, 8));

            var session = _context.DeviceSessions
                .Where(sess => sess.Token == token)
                .FirstOrDefault();

            if (session is null)
            {
                _logger.LogWarning("Invalid token attempt: {TokenPrefix}...", token.Substring(0, 8));
                return Unauthorized(new
                {
                    error = "Device token is not valid"
                });
            }

            var previousChannel = _context.ChannelHeaders.Where(ch => ch.DeviceSession.Token == token).FirstOrDefault();
            if (previousChannel != null)
            {
                //channel exists, delete it!!
                _logger.LogInformation("deleting old channel, was {}",previousChannel.Id);
                if(_waitingClients.TryGetValue(previousChannel.Id, out var clients))
                {
                    _logger.LogInformation("Dropped client");
                    clients.Tcs.SetCanceled();
                    _waitingClients.Remove(previousChannel.Id, out _);
                    ;
                }
                _context.ChannelHeaders.Remove(previousChannel);
                _context.SaveChanges();
            }


            var channelId = Guid.NewGuid().ToString();
            _logger.LogInformation("Creating new channel with ID: {ChannelId}", channelId);

            try
            {
                _context.ChannelHeaders.Add(
                    new Model.ChannelHeader
                    {
                        Id = channelId,
                        Token=token,
                        CreatedAt = DateTime.Now,
                        LastAccessAt = DateTime.Now,
                        DeviceSession = session,
                    }
                    );
                _context.SaveChanges();
                _logger.LogInformation("Successfully created channel: {ChannelId}", channelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create channel: {ChannelId}", channelId);
                return StatusCode(500, "Failed to create channel");
            }

            return Ok(new { channelId });
        }

        [HttpPost("listener/poll")]
        public async Task<ActionResult<ShadeStatusUpdateResponse>> PollForNotifications(
            [FromHeader(Name = "Authorization")] string token,
            [FromBody] Dto.Request.ShadeStateUpdateRequest body,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Polling request received for channel: {ChannelId}", body.channelId);

            if (!_deviceAuthService.IsValidToken(token))
            {
                _logger.LogWarning("Invalid token in polling request: {TokenPrefix}...", token.Substring(0, 8));
                return Unauthorized(new ErrorResponse { Message = $"'{token}' is not a valid token" });
            }

            var channelData = _context.ChannelHeaders.Where(ch => ch.Id == body.channelId && ch.DeviceSession.Token == token).FirstOrDefault();
            if (channelData is null)
            {
                _logger.LogWarning("Invalid channel ID in polling request: {ChannelId}", body.channelId);
                return BadRequest(new ErrorResponse{ Message = $"channelId '{body.channelId}' does not exist" });
            }

            try
            {
                var sessionMessages = _messages.Where(m => m.ChannelId == body.channelId).ToList();
                _logger.LogDebug("Found {Count} messages for channel {ChannelId}", sessionMessages.Count, body.channelId);

                if (sessionMessages.Any())
                {
                    Message message = null;
                    foreach (var msg in sessionMessages)
                    {
                        if (_messages.TryDequeue(out Message? _))
                        {
                            _logger.LogInformation("Extracted message out of message queue. Message ID {MessageId}", msg.MessageId);
                            message = msg;
                            break;
                        }
                    }

                    if (message != null)
                    {
                        _logger.LogInformation("Returning existing message for channel {ChannelId}. Message contents: {Content}", body.channelId, message.Content);
                        return new ShadeStatusUpdateResponse { Message=message.Content, MessageId=message.MessageId };
                    }
                }

                var tcs = new TaskCompletionSource<Message>();
                _logger.LogDebug("No immediate messages, setting up long polling for channel {ChannelId}", body.channelId);

                _waitingClients.TryAdd(channelData.Id, (tcs, channelData.Id));
                _logger.LogDebug("Client added to waiting list for channel {ChannelId}", body.channelId);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                try
                {
                    await using (cts.Token.Register(() => tcs.TrySetCanceled()))
                    {
                        var result = await tcs.Task;
                        _logger.LogInformation("Message received during polling for channel {ChannelId}", body.channelId);
                        return new ShadeStatusUpdateResponse { Message = result.Content, MessageId = result.MessageId };
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Polling timeout for channel {ChannelId}", body.channelId);
                    return StatusCode(408);
                }
                finally
                {
                    _waitingClients.TryRemove(channelData.Id, out _);
                    _logger.LogDebug("Removed client from waiting list for channel {ChannelId}", body.channelId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during polling for channel {ChannelId}", body.channelId);
                return StatusCode(500, new ErrorResponse { Message=ex.Message });
            }
        }

        //No RESTFUL method because time constraints
        [HttpPost("send")]
        public ActionResult<SendShadeStatusUpdateResponse> SendNotification(
            [FromBody] NotifyChannelListener body,
            [FromHeader(Name = "Authorization")] string userToken)
        {
            _logger.LogInformation("Sending notification to channel {ChannelId}", body.ChannelId);

            var message = new Message
            {
                Content = body.Message,
                MessageId = Guid.NewGuid().ToString(),
                ChannelId = body.ChannelId,
                Timestamp = DateTime.Now
            };

            if(!_context.ChannelHeaders.Any(ch => ch.Id == body.ChannelId))
            {
                return BadRequest(new ErrorResponse { Message = "invalid channel id" });
            }
            

            var notifiedClients = 0;
            foreach (var client in _waitingClients)
            {
                if (client.Value.ChannelId == body.ChannelId)
                {
                    //_messages.Where(m => m.ChannelId == body.ChannelId).FirstOrDefault();
                    client.Value.Tcs.TrySetResult(message);
                    notifiedClients++;
                }
            }
            _logger.LogInformation("Notified {Count} waiting clients for channel {ChannelId}", notifiedClients, body.ChannelId);

            if (notifiedClients == 0)
            {
                _messages.Enqueue(message);
                _logger.LogInformation("Message enqueued for channel {ChannelId}. Message ID: {MessageId}", body.ChannelId, message.MessageId);
            }
            
            return new SendShadeStatusUpdateResponse { MessageId = message.MessageId };
        }
    }
}