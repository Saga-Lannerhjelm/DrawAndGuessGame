using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        // [HttpPost]
        // public async Task<IActionResult> AddUser([FromBody]string username)
        // {
        //     //get random 
        //     var randomUsername = await _userRepository.GetRandomUsername();
        //     username = randomUsername;

        //     // Check game does not exist
        //     var users = _userRepository.GetAllUsers(out string error);

        //     if (!string.IsNullOrEmpty(error))
        //     {
        //         return BadRequest(error);
        //     }

        //     if (users.Count == 0)
        //     {
        //         return BadRequest(error);
        //     }
        //     return Ok(new {userId, username});
        // }

        [HttpGet]
        public IActionResult Getusers()
        {
            // Check game does not exist
            var users = _userRepository.GetAllUsers(out string error);

            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(error);
            }

            if (users.Count == 0)
            {
                return BadRequest("No users found");
            }

            return Ok(users);
        }
    }
}