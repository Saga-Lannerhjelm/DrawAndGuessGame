using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class GameRound
    {
        public int Id { get; set; }
        public string Word { get; set; } = "a word";
        public int RoundNr { get; set; } = 0;
        public bool RoundComplete { get; set; } = false;
        public int GameId { get; set; }
        // public List<User> Users { get; set; } = [];

    }
}