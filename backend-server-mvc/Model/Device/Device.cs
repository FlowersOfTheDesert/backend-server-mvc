using backend_server_mvc.Model.Session;
using backend_server_mvc.Model.Shade;
using System.Text.Json.Serialization;

namespace backend_server_mvc.Model.Device
{
    public class Device
    {
        
        public string Id { get; set; }
        public string Serial { get; set; }
        public string Label { get; set; }

        public ShadeStatus Status { get; set; }
        public string Psk { get; set; }
        public BaseShadeConfig? DeviceConfiguration { get; set; }
        
        
        public DeviceSession? Session { get; set; }

        //User relation
        public string? OwnerId { get; set; }
        
        public User? Owner { get; set; } = null;

    }
}
