using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Services;
using webbAPI.DataService;
using webbAPI.Hubs;
using webbAPI.Models;
using webbAPI.Repositories;

namespace webbAPI.Services
{
    public class GameBackgroundService(IHubContext<DrawHub> hubContext, ILogger<GameBackgroundService> logger, SharedDB sharedDB, GameRepository gameRepository, GameRoundRepository gameRoundRepository) : BackgroundService
    {
        private readonly IHubContext<DrawHub> _hubContext = hubContext;
        private readonly ILogger<GameBackgroundService> _logger = logger;
        private readonly SharedDB  _sharedDB = sharedDB;
        private readonly GameRepository _gameRepository = gameRepository;
        private readonly GameRoundRepository _gameRoundRepository = gameRoundRepository;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var games = _gameRepository.GetActiveGames(out string error) ?? new List<Game>();
                    // Console.WriteLine("get active games from DB");

                    if (games.Count != 0 || string.IsNullOrEmpty(error))
                    {
                        // Error: Transaction (Process ID 63) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.
                        foreach (var game in games)
                        {
                                var round = _gameRoundRepository.GetGameRoundByGameId(game.Id, out error) ?? new GameRound();
                                if (round.Id != 0 && !round.RoundComplete )
                                {
                                    if (round.Time > 0)
                                    {
                                        round.Time--;
                                        var affectedRows = _gameRoundRepository.Update(round, out error);
                                        Console.WriteLine("update time in DB");

                                        if (affectedRows != 0 || string.IsNullOrEmpty(error))
                                        {
                                            await _hubContext.Clients.Group(game.JoinCode).SendAsync("ReceiveTimerData", round.Time);
                                            if (round.Time <= 0)
                                            {
                                                Console.WriteLine("end");
                                                await _hubContext.Clients.Group(game.JoinCode).SendAsync("EndRound", game.JoinCode);
                                            }
                                        }
                                    }
                                }
                            }
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } 
        }
    }
}