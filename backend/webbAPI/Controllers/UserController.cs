using Microsoft.AspNetCore.Mvc;
using webbAPI.Repositories;

namespace webbAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(UserRepository userRepository) : ControllerBase
    {
        private readonly UserRepository _userRepository = userRepository;

        [HttpGet]
        public IActionResult Getusers()
        {
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