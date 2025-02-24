namespace backend_server_mvc.Dto.Response
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string? Stack { get; set; } = null;
    }
}
