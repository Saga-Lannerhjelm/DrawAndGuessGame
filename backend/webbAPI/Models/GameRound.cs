using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class GameRound
    {
        // public int Round { get; set; } = 0;
        public string Word { get; set; } = "a word";
        public bool RoundComplete { get; set; } = false;
        public List<User> Users { get; set; } = [];

    }
}