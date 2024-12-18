using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class UserConnection
    {
        public string Username { get; set; } = string.Empty;
        public string GameRoom { get; set; } = string.Empty;
        public bool IsDrawing { get; set; } = false;
        public int Points { get; set; } = 0;
        public bool HasGuessedCorrectly { get; set; } = false;
    }
}