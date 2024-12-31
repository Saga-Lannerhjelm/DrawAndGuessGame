using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webbAPI.Models.ViewModels
{
    public class UserVM
    {
        public User User { get; set; } = new User();
        public UserInRound UserInRound { get; set; } = new UserInRound();
    }
}