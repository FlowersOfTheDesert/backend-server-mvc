namespace backend_server_mvc.Model.Session
{
    public class DeviceSession
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public DateTime IssuedOn { get; set; }
        public int TTL { get; set; }
        public string DeviceId { get; set; }
        public Device.Device? Device { get; set; }    // Nullable navigation property
        public ChannelHeader? Channel { get; set; }
    }
}
