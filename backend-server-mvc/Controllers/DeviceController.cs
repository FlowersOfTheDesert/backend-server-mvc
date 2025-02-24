using backend_server_mvc.Data;
using backend_server_mvc.Dto.Response;
using backend_server_mvc.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_server_mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        AppDbContext _context;
        ILogger<DeviceController> _logger;
        IUserSessionAuthService _userAuthService;
        public DeviceController(AppDbContext context, ILogger<DeviceController> logger, IUserSessionAuthService userAuthService)
        {
            _logger = logger;
            _context = context;
            _userAuthService = userAuthService;
        }

        [HttpGet]
        public ActionResult<List<Dto.Response.DeviceResponse>> GetUserDevices([FromHeader(Name="Authorization")]string sessionId)
        {
            var user = _userAuthService.UserFromSessionId(sessionId);
            if(user == null)
            {
                _logger.LogWarning($"session id {sessionId} is not valid");
                return Unauthorized(new ErrorResponse { Message=$"session id {sessionId} is not valid"});
            }
            return user.OwnedDevices.Select(d => new DeviceResponse
            {
                Id = d.Id,
                Serial = d.Serial,
                Label = d.Label,
                OwnerId = d.OwnerId,
            }).ToList();

        }
    }
}
