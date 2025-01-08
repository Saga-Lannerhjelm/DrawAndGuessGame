using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webbAPI.DataService;
using webbAPI.Models;
using webbAPI.Repositories;

namespace webbAPI.Controllers;

[Authorize]
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

        if (existingGame?.Id == 0 || string.IsNullOrEmpty(error))
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
    }

    [HttpPost ("room")]
    public IActionResult IsGameExisting([FromBody] string joinCode)
    {
        // Check game does not exist
        string error = "";
        var existingGame = _gameRepository.GetGameByJoinCode(joinCode, out error);

        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(error);
        } 

        return Ok(existingGame.Id != 0 && existingGame.IsActive == false);
    }
}
