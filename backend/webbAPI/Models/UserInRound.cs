using System.ComponentModel.DataAnnotations;

namespace webbAPI.Models
{
    public class UserInRound
    {
        public int Id { get; set; }
        [Required]
        public bool IsDrawing { get; set; } = false;
        [Required]
        public int Points { get; set; } = 0;
        [Required]
        public bool GuessedCorrectly { get; set; } = false;
        [Required]
        public bool GuessedFirst { get; set; } = false;
        [Required]
        public int UserId { get; set; }
        [Required]
        public int GameRoundId { get; set; }
    }
}