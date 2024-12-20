using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class Game
    {
        public string RoomName { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public bool HasStarted { get; set; } = false;
        public List<GameRound> Rounds { get; set; } = [];
    }
}