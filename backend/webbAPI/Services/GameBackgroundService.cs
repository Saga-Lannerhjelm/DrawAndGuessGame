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

                    if (games.Count != 0 || string.IsNullOrEmpty(error))
                    {
                        foreach (var game in games)
                        {
                            var round = _gameRoundRepository.GetGameRoundByGameId(game.Id, out error) ?? new GameRound();
                            if (round.Id != 0 && !round.RoundComplete )
                            {
                                var currentTime =  TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")); 
                                var roundStartTime = round.StartTime;
                                var pastSeconds = Math.Floor((currentTime - roundStartTime).TotalSeconds);
                                var roundTime = 30;

                                if (pastSeconds <= roundTime)
                                {
                                    var timerValue = roundTime - pastSeconds;
                                    await _hubContext.Clients.Group(game.JoinCode).SendAsync("ReceiveTimerData", timerValue);
                                    if (timerValue <= 0)
                                    {
                                         var users = _sharedDB.Connection
                                        .Where(g => g.Value.JoinCode == game.JoinCode).ToList();
                                        var userConnectionId = users.Select((users) => users.Key).ToList()[0];
                                        await _hubContext.Clients.Client(userConnectionId).SendAsync("EndRound", game.JoinCode);
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