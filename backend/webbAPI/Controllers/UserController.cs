using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using webbAPI.DataService;
using webbAPI.Models;
using webbAPI.Repositories;

namespace webbAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(SharedDB sharedDB, UserRepository userRepository) : ControllerBase
    {
        private readonly SharedDB _sharedDB = sharedDB;
        private readonly UserRepository _userRepository = userRepository;

        [HttpPost]
        public IActionResult AddUser([FromBody] string username)
        {
            // Check game does not exist
            var user = new User
            {
                Username = username
            };
            int userId = _userRepository.Insert(user, out string error);

            if (userId == 0 || !string.IsNullOrEmpty(error))
            {
                return BadRequest();
            }
            return Ok(userId);
        }
    }
}