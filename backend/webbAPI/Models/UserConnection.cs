namespace webbAPI.Models
{
    public class UserConnection
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
    }
}