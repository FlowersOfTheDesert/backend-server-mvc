namespace backend_server_mvc.Model.Shade
{
    public class BaseShadeConfig
    {
        public string Id { get; set; }
        public string DeviceId { get; set; }  // Nullable foreign key
        public Device.Device? Device { get; set; }    // Nullable navigation property
    }
}
