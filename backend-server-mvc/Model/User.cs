using backend_server_mvc.Model.Device;
using backend_server_mvc.Model.Session;

namespace backend_server_mvc.Model
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; }
        public ICollection<Device.Device>? OwnedDevices { get; set; } = new List<Device.Device>();
        public ICollection<UserSession>? Session { get; set; } = new List<UserSession>();
    }
}
