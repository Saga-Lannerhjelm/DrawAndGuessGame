using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class User
    {
        public UserConnection UserDetails { get; set; } = new();
        public bool IsDrawing { get; set; } = false;
        public int Points { get; set; } = 0;
        public bool HasGuessedCorrectly { get; set; } = false;
        public bool GuessedFirst { get; set; } = false;
    }
}