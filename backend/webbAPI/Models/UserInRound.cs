using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class UserInRound
    {
        public int Id { get; set; }
        public bool IsDrawing { get; set; } = false;
        public int Points { get; set; } = 0;
        public bool GuessedCorrectly { get; set; } = false;
        public bool GuessedFirst { get; set; } = false;
        public int UserId { get; set; }
        public int GameRoundId { get; set; }
    }
}