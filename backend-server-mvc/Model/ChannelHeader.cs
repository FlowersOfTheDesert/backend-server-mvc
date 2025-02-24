using backend_server_mvc.Model.Session;

namespace backend_server_mvc.Model
{
    public class ChannelHeader
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessAt { get; set; }

        public string DeviceSessionId { get; set; }
        public DeviceSession DeviceSession { get; set; } = null!;
    }
}
