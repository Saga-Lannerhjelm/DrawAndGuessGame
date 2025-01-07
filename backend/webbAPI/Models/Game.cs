using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models
{
    public class Game
    {
        public int Id { get; set; }
        [Required]
        public string RoomName { get; set; } = string.Empty;
        [Required]
        [StringLength(8, ErrorMessage = "Anslutningskoden måste vara 8 tecken lång")]
        public string JoinCode { get; set; } = string.Empty;

        [Range(3, 10, ErrorMessage = "Antalet rundor måste vara emllan 3 och 10")]
        public int Rounds { get; set; } = 3;
        public bool IsActive { get; set; } = false;
        public int CreatorId { get; set; }
    }
}