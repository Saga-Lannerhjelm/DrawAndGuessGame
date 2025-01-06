using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public int Rounds { get; set; }
        public bool IsActive { get; set; } = false;
        public int CreatorId { get; set; }
    }
}