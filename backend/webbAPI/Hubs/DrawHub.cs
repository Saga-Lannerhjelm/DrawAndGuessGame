using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using webbAPI.DataService;
using webbAPI.Models;

namespace webbAPI.Hubs
{
    public class DrawHub : Hub
    {   
        private readonly SharedDB _sharedDB;

        public DrawHub (SharedDB sharedDB)
        {
            _sharedDB = sharedDB;
        }
        public async Task JoinGame (UserConnection userConn) 
        {
            // If game exist
            if (_sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom && exGame.IsActive == false) != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConn.GameRoom);
                // Add user (to game)
                _sharedDB.Connection[Context.ConnectionId] = userConn;


                await Clients.OthersInGroup(userConn.GameRoom).SendAsync("GameStatus", $"{userConn.Username} anslöt till spelet", true);
                await Clients.Caller.SendAsync("GameStatus", $"Välkommen till spelet. Anslutningkoden är {userConn.GameRoom}", true);

                await UsersInGame(userConn.GameRoom);
                // await GameInfo(userConn.GameRoom);
            } else {
                await Clients.Caller.SendAsync("GameStatus", $"Spelet finns inte eller har redan startat", false);
            }
        }

        public async Task StartRound(string gameRoom)
        {
            // Get users in game
            var users = _sharedDB.Connection
            .Where(g => g.Value.GameRoom == gameRoom).ToList();

            int usersInGameNr = users.Count;

            var allUsersInGame = _sharedDB.Connection.Values
            .Where(g => g.GameRoom == gameRoom).ToList();

            try
            {
                if (usersInGameNr >= 3)
                {
                    // Current game
                    // Get game by join code
                    var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == gameRoom);

                    if (currentGame != null) 
                    {
                        // Mark game as started
                        //Update game to active
                        currentGame.IsActive = true;

                        // Add new Round
                        // currentGame.Rounds.Add(new GameRound{});

                        // Add users to the round
                        foreach (var user in allUsersInGame)
                        {
                            // currentGame.Rounds[^1].Users.Add(new User{UserDetails = user});
                        }

                        // Get word
                        try
                        {
                            await GetWord(currentGame);  
                        }
                        catch (Exception ex)
                        {
                            
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                    
                        // Select players to draw
                        var rnd = new Random();

                        int randomNr1 = rnd.Next(usersInGameNr);
                        int randomNr2 = rnd.Next(usersInGameNr);

                        while (randomNr2 == randomNr1)
                        {
                            randomNr2 = rnd.Next(usersInGameNr);
                        }

                        // var selectedUser1 = currentGame.Rounds[^1].Users[randomNr1];
                        // var selectedUser2 = currentGame.Rounds[^1].Users[randomNr2];

                        // selectedUser1.IsDrawing = true;
                        // selectedUser2.IsDrawing = true;

                        // Make sure that only the ones that hasn't drawn yet are selected
                        // Maybe filter out the ones that already have drawn

                        await Clients.Group(gameRoom).SendAsync("GameCanStart", true);
                        // await Clients.Group(gameRoom).SendAsync("GameStatus", $"{selectedUser1.UserDetails.Username} och {selectedUser2.UserDetails.Username} ritar!");
                        // await UsersInRound(gameRoom);
                        // await GameInfo(gameRoom);
                     
                    }
                    else 
                    {
                        await Clients.Group(gameRoom).SendAsync("GameCanStart", false);
                    }
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task GetWord (Game currentGame) 
        {
            // Link to API https://random-word-form.herokuapp.com
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync("https://random-word-form.herokuapp.com/random/noun");
            string word = "Default word";

            if (response.IsSuccessStatusCode)
            {
                string apiResp = await response.Content.ReadAsStringAsync();
                string[] words = JsonConvert.DeserializeObject<string[]>(apiResp) ?? [];
                word = (words?.Length > 0) ? words[0] : "";
            } 
            // currentGame.Rounds[^1].Word = word;
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
                .Where(g => g.Value.GameRoom == gameRoom).ToList();
                var userValues = users.Select((users) => users.Value);

                return Clients.Group(gameRoom).SendAsync("UsersInGame", userValues);
                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // public Task UsersInRound (string gameRoom) 
        // {
        //     var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == gameRoom);
        //     var currentRound = currentGame?.Rounds[^1];
        //     var users = currentGame?.Rounds[^1].Users.ToList();

        //     return Clients.Group(gameRoom).SendAsync("UsersInGame", users);
        // }

        // public Task GameInfo(string gameRoom) 
        // {
        //     var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == gameRoom);
        //     var currentRound = currentGame?.Rounds.Count > 0 ? currentGame?.Rounds[^1] : new GameRound(); 

        //     return Clients.Group(gameRoom).SendAsync("receiveGameInfo", currentGame, currentRound);
        // }

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
                var game = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom);
                if (_sharedDB.CreatedGames.TryTake(out game))
                {
                    await Clients.Group(userConn.GameRoom).SendAsync("leaveGame");
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