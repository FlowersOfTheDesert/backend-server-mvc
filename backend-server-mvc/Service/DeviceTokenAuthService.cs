using backend_server_mvc.Data;

namespace backend_server_mvc.Service
{
    public interface IDeviceTokenAuthService
    {
        public bool IsValidToken(string token);
    }

    public class DeviceTokenAuthService : IDeviceTokenAuthService
    {
        private readonly AppDbContext _context;

        public DeviceTokenAuthService(AppDbContext context)
        {
            _context = context;
        }

        public bool IsValidToken(string token)
        {
            return _context.DeviceSessions.Any(session => 
                session.Token == token && (session.IssuedOn.AddSeconds(session.TTL) > DateTime.Now)
            );
        }
    }
}
