using Microsoft.AspNetCore.Mvc;
using webbAPI.DataService;
using webbAPI.Models;

namespace webbAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly SharedDB _sharedDB;

    public GameController (SharedDB sharedDB)
    {
        _sharedDB = sharedDB;
    }

    [HttpPost]
    public IActionResult CreateGame([FromBody] Game game)
    {
        if (_sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.RoomName == game.RoomName || exGame.JoinCode == game.JoinCode) == null)
        {
            _sharedDB.CreatedGames.Add(new Game {RoomName = game.RoomName, JoinCode = game.JoinCode});
            return Ok();
            
        } else
        {
            return BadRequest();;
        }
        
    }
}
