using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class UserConnection
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        // public int TotalPoints { get; set; } = 0;
        // public int Winns { get; set; } = 0;
    }
}