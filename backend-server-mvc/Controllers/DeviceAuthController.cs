using backend_server_mvc.Data;
using backend_server_mvc.Model.Session;
using backend_server_mvc.Service;
using backend_server_mvc.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace backend_server_mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceAuthController : ControllerBase
    {
     
        // Stores active challenges (nonce) for each device
        private static readonly ConcurrentDictionary<string, string> PendingChallenges = new();
        private AppDbContext _context;
        private ILogger<DeviceAuthController> _logger;
       
        public DeviceAuthController(AppDbContext context, ILogger<DeviceAuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("challenge")]
        public ActionResult<Dto.Response.AuthChallengeResponse> GetChallenge([FromBody]Dto.Request.RequestAuthChallenge request)
        {
            _logger.LogInformation($"Device {request.deviceId} requesting authentication challenge");
            //Check the device actually exists
            if (!_context.Devices.Any(d => d.Id == request.deviceId))
            {
                _logger.LogInformation($"Rejected device challenge request (deviceId {request.deviceId} not valid)");
                return Unauthorized(new {error = "Unknown device (deviceId not valid)"});
            }


            var challenge = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            PendingChallenges[request.deviceId] = challenge;
            _logger.LogInformation("Issued challenge: " + challenge);
            return new Dto.Response.AuthChallengeResponse { challenge=challenge };
   
        }

        [HttpPost("respond")]
        public ActionResult RespondChallenge([FromBody] Dto.Request.AnswerAuthChallenge request)
        {
            //var device =_context.Devices.ToList().Find(d => d.Id == request.deviceId);
            //var psk = device?.Psk;
            _logger.LogInformation($"Device authentication challenge response from client {request.deviceId}");

            var device = _context.Devices.Where(d => d.Id == request.deviceId).FirstOrDefault();
            var psk = device?.Psk;
            if(device == null || !PendingChallenges.TryRemove(request.deviceId, out var challenge))
            {
                _logger.LogInformation("Rejected request. Invalid or expired challenge");
                return Unauthorized(new { error = "Invalid or expired challenge" });
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(psk!));
            var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(challenge)));
            if (request.challengeResponse.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                //invalidate previous session if it exists
                var existingSession = _context.DeviceSessions.Where(d => d.DeviceId == device.Id).FirstOrDefault();
                if(existingSession != null)
                {
                    var oldChannel = _context.ChannelHeaders.Where(c => c.DeviceSession.Token == existingSession.Token).FirstOrDefault();
                    if (oldChannel != null)
                    {
                        _logger.LogInformation($"Invalidating existing notification channel (id: {oldChannel.Id})");
                        _context.ChannelHeaders.Remove(oldChannel);
                    }
                    _logger.LogInformation($"Invalidating previous session (id: {existingSession.Id})");
                    _context.DeviceSessions.Remove(existingSession);
                }
                
                //save session data to the db
                var token = TokenGenerator.GenerateToken();
                _context.DeviceSessions.Add(new DeviceSession { 
                    Id=  Guid.NewGuid().ToString(),
                    Device=device,
                    Token = token,
                    TTL = 86400, //24 Hours
                    IssuedOn = DateTime.Now,
                });
                _context.SaveChanges();

                _logger.LogInformation($"device identity validated (id: {request.deviceId}). issued token '{token}'");
                return Ok(new { token = token});
            }

            _logger.LogInformation($"Authentication failed. Challenge response mismatch {expected} != {request.challengeResponse}");
            return Unauthorized(new { error = "Authentication failed" });

        }
    }
}
