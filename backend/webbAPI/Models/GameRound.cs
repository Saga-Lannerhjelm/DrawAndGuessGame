using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class GameRound
    {
        public int Id { get; set; }
        [Required]
        public string Word { get; set; } = "a word";
        [Required]
        public int RoundNr { get; set; } = 0;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public bool RoundComplete { get; set; } = false;
        [Required]
        public int GameId { get; set; }
    }
}