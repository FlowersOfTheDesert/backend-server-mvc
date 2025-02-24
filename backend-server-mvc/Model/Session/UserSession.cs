namespace backend_server_mvc.Model.Session
{
    public class UserSession
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
