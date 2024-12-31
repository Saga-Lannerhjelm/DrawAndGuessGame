using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using webbAPI.DataService;
using webbAPI.Models;
using webbAPI.Repositories;

namespace webbAPI.Hubs
{
    public class DrawHub : Hub
    {   
        private readonly SharedDB _sharedDB;
        private readonly GameRepository _gameRepository;
        private readonly GameRoundRepository _gameRoundRepository;
        private readonly UserRepository _userRepository;

        public DrawHub (SharedDB sharedDB, GameRepository gameRepository, GameRoundRepository gameRoundRepository, UserRepository userRepository)
        {
            _sharedDB = sharedDB;
            _gameRepository = gameRepository;
            _gameRoundRepository = gameRoundRepository;
            _userRepository = userRepository;
        }
        public async Task JoinGame (UserConnection userConn) 
        {
            // If game exist
            var existingGame = _gameRepository.GetGameByJoinCode(userConn.JoinCode, out string error);

            if (existingGame != null || string.IsNullOrEmpty(error))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConn.JoinCode);
                // Add user (to game)
                _sharedDB.Connection[Context.ConnectionId] = userConn;

                 await Clients.OthersInGroup(userConn.JoinCode).SendAsync("GameStatus", $"{userConn.Username} anslöt till spelet", true);
                await Clients.Caller.SendAsync("GameStatus", $"Välkommen till spelet. Anslutningkoden är {userConn.JoinCode}", true);

                await UsersInGame(userConn.JoinCode);
                // await GameInfo(userConn.GameRoom);
            }
            else 
            {
                await Clients.Caller.SendAsync("GameStatus", $"Spelet finns inte eller har redan startat", false);
            }
            // if (_sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.JoinCode && exGame.IsActive == false) != null)
            // {
            //     await Groups.AddToGroupAsync(Context.ConnectionId, userConn.JoinCode);
            //     // Add user (to game)
            //     _sharedDB.Connection[Context.ConnectionId] = userConn;


            //     await Clients.OthersInGroup(userConn.JoinCode).SendAsync("GameStatus", $"{userConn.Username} anslöt till spelet", true);
            //     await Clients.Caller.SendAsync("GameStatus", $"Välkommen till spelet. Anslutningkoden är {userConn.JoinCode}", true);

            //     await UsersInGame(userConn.JoinCode);
            //     // await GameInfo(userConn.GameRoom);
            // } else {
            //     await Clients.Caller.SendAsync("GameStatus", $"Spelet finns inte eller har redan startat", false);
            // }
        }

        public async Task StartRound(string joinCode)
        {
            // Get users in game
            // var users = _sharedDB.Connection
            // .Where(g => g.Value.JoinCode == joinCode).ToList();

            var allUsersInGame = _sharedDB.Connection.Values
            .Where(g => g.JoinCode == joinCode).ToList();

            int usersInGameNr = allUsersInGame.Count;

            try
            {
                if (usersInGameNr >= 3)
                {
                    // Current game
                    // Get game by join code
                    string error = "";
                    var currentGame = _gameRepository.GetGameByJoinCode(joinCode, out error);

                    if (currentGame != null || string.IsNullOrEmpty(error))
                    {
                        // Update gamestate to active
                        var affectedRows = _gameRepository.UpdateActiveState(currentGame.Id, true, out error);

                        if (affectedRows == 0 || !string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }
                        currentGame.IsActive = true;

                        // Get word
                        string word = "default word";
                        try
                        {
                            word = await GetWord();  
                        }
                        catch (Exception ex)
                        {
                            
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }

                        // Add a new round
                        var newGameRound = new GameRound {
                            GameId = currentGame.Id,
                            Word = word,
                        };
                        
                        var roundId = _gameRoundRepository.Insert(newGameRound, out error);

                        if (roundId == 0 || !string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }
                        var usersInRoundList = new List<UserInRound>();

                        foreach (var user in allUsersInGame)
                        {
                            usersInRoundList.Add(new UserInRound {
                                UserId = user.Id,
                                IsDrawing = false,
                                GameRoundId = roundId,
                            });
                        }

                        // Select users to draw
                        var rnd = new Random();

                        int randomNr1 = rnd.Next(usersInGameNr);
                        int randomNr2 = rnd.Next(usersInGameNr);

                        while (randomNr2 == randomNr1)
                        {
                            randomNr2 = rnd.Next(usersInGameNr);
                        }

                        var drawingUserOne = allUsersInGame[randomNr1];
                        var drawingUserTwo = allUsersInGame[randomNr2];

                        usersInRoundList[randomNr1].IsDrawing = true;
                        usersInRoundList[randomNr2].IsDrawing = true;

                        // Make sure that only the ones that hasn't drawn yet are selected
                        // Maybe filter out the ones that already have drawn

                        // Add users to the round
                        foreach (var userInRound in usersInRoundList)
                        {
                            affectedRows = _userRepository.InsertUserInRound(userInRound, out error);

                            if (affectedRows == 0 || !string.IsNullOrEmpty(error))
                            {
                                throw new Exception(error);
                            }
                        }
                        await Clients.Group(joinCode).SendAsync("GameCanStart", true);
                        await Clients.Group(joinCode).SendAsync("GameStatus", $"{drawingUserOne.Username} och {drawingUserTwo.Username} ritar!");
                        await UsersInRound(roundId, currentGame.JoinCode);
                        // await GameInfo(gameRoom);
                        // currentGame.Rounds[^1].Users.Add(new User{UserDetails = user});
                        
                    }
                    else {
                         throw new Exception(error);
                    }
                   
                }
                else 
                {
                    await Clients.Group(joinCode).SendAsync("GameCanStart", false);
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task<string> GetWord () 
        {
            // Link to API https://random-word-form.herokuapp.com
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync("https://random-word-form.herokuapp.com/random/noun");

            if (response.IsSuccessStatusCode)
            {
                string apiResp = await response.Content.ReadAsStringAsync();
                string[] words = JsonConvert.DeserializeObject<string[]>(apiResp) ?? [];
                return (words?.Length > 0) ? words[0] : "default word";
            } else {
                return "Default word";
            }
        }
        public async Task Drawing(Point start, Point end, string color, string gameRoom) 
        {
            await Clients.OthersInGroup(gameRoom).SendAsync("Drawing", start, end, color);
        }

        public async Task SendGuess(string guess)
        {
            // if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            // {
            //      var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom);
            //      var currentRound = currentGame?.Rounds[^1];
            //      var activeUser = currentGame?.Rounds[^1].Users.Find(u => u.UserDetails.Username == userConn.Username) ?? new User();

            //     if (guess == currentRound?.Word)
            //     {

            //         var users = currentRound.Users.Where(g => g.UserDetails.GameRoom == userConn.GameRoom).ToList();

            //         if (!users.Where(user => user.HasGuessedCorrectly).Any())
            //         {
            //             activeUser.GuessedFirst = true;
            //         }

            //         activeUser.HasGuessedCorrectly = true;

            //         if (!users.Where(user => !user.IsDrawing && !user.HasGuessedCorrectly).Any())
            //         {
            //             await EndRound(currentRound, userConn.GameRoom);
            //         }

            //         await UsersInRound(userConn.GameRoom);
            //         await GameInfo(userConn.GameRoom);
            //         await Clients.Caller.SendAsync("ReceiveGuess", guess, userConn.Username);
            //         await Clients.OthersInGroup(userConn.GameRoom).SendAsync("ReceiveGuess", "Gissade rätt", userConn.Username);
            //     } else {
            //         await Clients.Group(userConn.GameRoom).SendAsync("ReceiveGuess", guess, userConn.Username);
            //     }
            // }

        }

        public Task UsersInGame(string gameRoom) 
        {
            try
            {
                var users = _sharedDB.Connection
                .Where(g => g.Value.JoinCode == gameRoom).ToList();
                var userValues = users.Select((users) => users.Value);

                return Clients.Group(gameRoom).SendAsync("UsersInGame", userValues);
                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task UsersInRound (int roundId, string joinCode) 
        {
            var users = _userRepository.GetUsersByRound(roundId, out string error);

            if (users == null || !string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Errir: ", error);
                return Clients.Group(joinCode).SendAsync("UsersInGame", null);  
            }
            return Clients.Group(joinCode).SendAsync("UsersInGame", users);  
        }

        public Task GameInfo(string joinCode) 
        {
            // var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == gameRoom);
            // var currentRound = currentGame?.Rounds.Count > 0 ? currentGame?.Rounds[^1] : new GameRound(); 

            string error = "";
            var currentGame = new Game();
            var currentRound = new GameRound();

            try
            {
                currentGame = _gameRepository.GetGameByJoinCode(joinCode, out error);

                if (currentGame != null && string.IsNullOrEmpty(error))
                {
                    currentRound = _gameRoundRepository.GetGameRoundByGameId(currentGame.Id, out error);

                    if (currentRound == null && !string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:", ex);
            }
            return Clients.Group(joinCode).SendAsync("receiveGameInfo", currentGame, currentRound);

        }

        public async Task SendClearCanvas (string gameRoom) 
        {
            await Clients.Group(gameRoom).SendAsync("clearCanvas");
        }

        // public Task EndRound (GameRound round, string gameRoom) 
        // {
        //     round.RoundComplete = true;

        //     var winner = round.Users.Find(user => user.GuessedFirst);
        //     if (winner != null) winner.Points = 5;

        //     var usersGuessedCorrectly = round.Users.FindAll(user => !user.GuessedFirst && !user.IsDrawing && user.HasGuessedCorrectly);
            
        //     foreach (var user in usersGuessedCorrectly)
        //     {
        //         user.Points = 3;
        //     }

        //     var artists = round.Users.FindAll(user => user.IsDrawing);

        //     foreach (var user in artists)
        //     {
        //         user.Points = 4;
        //     }
        //     return Clients.Group(gameRoom).SendAsync("RoundEnded");
        // }
        
        public async Task EndGame () 
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                var game = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.JoinCode);
                if (_sharedDB.CreatedGames.TryTake(out game))
                {
                    await Clients.Group(userConn.JoinCode).SendAsync("leaveGame");
                } 
            }
        }

        // public async Task SendTimerData (int timerValue)
        // {
        //     if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
        //     {
        //         while (timerValue >= 0)
        //         {
        //             timerValue--;
        //             if (timerValue == 0)
        //             {
        //                 var currentRound = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom).Rounds[^1];
        //                 await EndRound(currentRound, userConn.GameRoom);
        //                 await GameInfo(userConn.GameRoom);
        //                 await UsersInGame(userConn.GameRoom);
        //             }    
        //             await Clients.Group(userConn.GameRoom).SendAsync("ReceiveTimerData", timerValue);
        //             await Task.Delay(1000);
        //         }

        //     }
        // }

        // public override Task OnDisconnectedAsync(Exception? exception)
        // {
        //      if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
        //     {
        //         _sharedDB.Connection.Remove(Context.ConnectionId, out _);
        //         Clients.Group(userConn.GameRoom).SendAsync("GameStatus", $"{userConn.Username} har lämnat spelet");


        //         if (_sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom).IsActive)
        //         {
        //             var users = _sharedDB.CreatedGames?.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom).Rounds[^1].Users;
        //             var test = users?.Find(u => u.UserDetails == userConn) ?? new();
        //             users?.Remove(test);
        //             UsersInRound(userConn.GameRoom);    
        //         }
        //         else {
        //             UsersInGame(userConn.GameRoom);    
        //         }
        //     }
        //     return base.OnDisconnectedAsync(exception);
        // }
    }
}