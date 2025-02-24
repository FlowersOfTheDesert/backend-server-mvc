namespace backend_server_mvc.Dto.Response
{
    public class DeviceSessionResponse
    {
        public required string token { get; set; }
        public required int ttl { get; set; } 
    }
}
