namespace backend_server_mvc.Dto.Request
{
    public class AnswerAuthChallenge
    {
        public required string deviceId { get; set; }
        public required string challengeResponse { get; set; }
    }
}
