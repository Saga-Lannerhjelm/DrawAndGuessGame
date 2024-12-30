using Microsoft.AspNetCore.Mvc;
using webbAPI.DataService;
using webbAPI.Models;
using webbAPI.Repositories;

namespace webbAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly SharedDB _sharedDB;
    private readonly GameRepository _gameRepository;

    public GameController (SharedDB sharedDB, GameRepository gameRepository)
    {
        _sharedDB = sharedDB;
        _gameRepository = gameRepository;
    }

    [HttpPost]
    public IActionResult CreateGame([FromBody] Game game)
    {
        // Check game does not exist
        string error = "";
        var existingGame = _gameRepository.GetGameByJoinCode(game.JoinCode, out error);

        if (existingGame == null || string.IsNullOrEmpty(error))
        {
            var affectedRows = _gameRepository.Insert(game, out error);
            if (affectedRows == 0 || !string.IsNullOrEmpty(error))
            {
                return BadRequest();
            }
            return Ok();
        } else 
        {
            return BadRequest();
        }
        // if (_sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.RoomName == game.RoomName || exGame.JoinCode == game.JoinCode) == null)
        // {
        //     // Add user
            
        //     // Create new game
        //     _sharedDB.CreatedGames.Add(new Game {RoomName = game.RoomName, JoinCode = game.JoinCode});
        //     return Ok();
            
        // } else
        // {
        //     return BadRequest();;
        // }
        
    }
}
