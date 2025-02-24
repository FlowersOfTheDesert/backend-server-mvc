using backend_server_mvc.Data;
using backend_server_mvc.Dto.Request;
using backend_server_mvc.Dto.Response;
using backend_server_mvc.Util;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_server_mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private AppDbContext _context;

        public SessionController(AppDbContext context)
        {
            _context = context;
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin body)
        {
            var user = await _context.Users
                .Where(u => u.Username == body.Username)
                .FirstOrDefaultAsync();

            if (user == null || !PasswordHelper.VerifyPassword(body.Password, user.Password, user.Salt))
            {
                return Unauthorized(new ErrorResponse{ Message = "Invalid username or password" });
            }

            //create session
            var sessionId = TokenGenerator.GenerateToken();
            _context.UserSessions.Add(new Model.Session.UserSession { Id = sessionId, User = user });
            _context.SaveChanges();

            // Generate a token or session for authentication (JWT, session, etc.)
            return Ok(new { SessionId = sessionId});
        }

        
    }
}
