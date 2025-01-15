using System.ComponentModel.DataAnnotations;

namespace webbAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;
        public int TotalPoints { get; set; } = 0;
        public int Wins { get; set; } = 0;
        
        [Required (ErrorMessage = "Fältet kan inte vara tomt")]
        public string Password { get; set; } = string.Empty;

        public string Salt { get; set;} = string.Empty;
    }
}