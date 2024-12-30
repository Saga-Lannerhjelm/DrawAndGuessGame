using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public int TotalPoints { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int ActiveGameId { get; set; }
    }
}