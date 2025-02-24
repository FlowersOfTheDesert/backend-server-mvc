using backend_server_mvc.Data;
using backend_server_mvc.Model;
using Microsoft.EntityFrameworkCore;

namespace backend_server_mvc.Service
{

    public interface IUserSessionAuthService
    {
        public User? UserFromSessionId(string sessionId);
        public bool SessionIsValid(string sessionId);

    }
    public class UserSessionAuthService : IUserSessionAuthService
    {
        private AppDbContext _context;
        private ILogger<UserSessionAuthService> _logger;

        public UserSessionAuthService(AppDbContext context, ILogger<UserSessionAuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool SessionIsValid(string sessionId)
        {
            return _context.UserSessions.Any(s => s.Id == sessionId);
        }

        public User? UserFromSessionId(string sessionId)
        {
            _logger.LogInformation("Fetching user for session ID: {SessionId}", sessionId);

            var session = _context.UserSessions
                .Where(s => s.Id == sessionId)
                .Include(s => s.User)
                .FirstOrDefault();

            if (session == null)
            {
                _logger.LogWarning("No session found for session ID: {SessionId}", sessionId);
                return null;
            }

            if (session.User == null)
            {
                _logger.LogWarning("Session found but no associated user for session ID: {SessionId}", sessionId);
            }
            else
            {
                _logger.LogInformation("User {UserId} retrieved for session ID: {SessionId}", session.User.Id, sessionId);
            }

            return _context.Users
                .Include(u => u.OwnedDevices)
                .First(u => u.Id == session!.User!.Id);
           // return session.User;
        }

    }
}
