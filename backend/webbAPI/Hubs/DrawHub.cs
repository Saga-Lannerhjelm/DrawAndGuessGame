using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            if (_sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom && exGame.HasStarted == false) != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConn.GameRoom);
                _sharedDB.Connection[Context.ConnectionId] = userConn;


                await Clients.OthersInGroup(userConn.GameRoom).SendAsync("GameStatus", $"{userConn.Username} anslöt till spelet", true);
                await Clients.Caller.SendAsync("GameStatus", $"Välkommen till spelet. Anslutningkoden är {userConn.GameRoom}", true);

                await UsersInGame(userConn.GameRoom);
                await GameInfo(userConn.GameRoom);
            } else {
                await Clients.Caller.SendAsync("GameStatus", $"Spelet finns inte eller har redan startat", false);
            }
        }

        public async Task StartRound(string gameRoom)
        {
            var users = _sharedDB.Connection
            .Where(g => g.Value.GameRoom == gameRoom).ToList();

            int usersInGame = users.Count;

            try
            {
                if (usersInGame >= 3)
            {
                // Current game
                var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == gameRoom);


                // Mark game as started
                if (currentGame != null) 
                {
                    currentGame.HasStarted = true;

                    currentGame.Rounds.Add(new GameRound {
                        Round =+ 1,
                    });

                    // Get word
                    await GetWord(currentGame);
                }

                
                // Select players to draw
                var rnd = new Random();

                int randomNr1 = rnd.Next(usersInGame);
                int randomNr2 = rnd.Next(usersInGame);

                while (randomNr2 == randomNr1)
                {
                    randomNr2 = rnd.Next(usersInGame);
                }

                var selectedUser1 = users[randomNr1].Value;
                var selectedUser2 = users[randomNr2].Value;

                selectedUser1.IsDrawing = true;
                selectedUser2.IsDrawing = true;

                var drawingUserOne = selectedUser1.Username;
                var drawingUserTwo = selectedUser2.Username;

                // Make sure that only the ones that hasn't drawn yet are selected
                // Maybe filter out the ones that already have drawn

                await Clients.Group(gameRoom).SendAsync("GameCanStart", true);
                await Clients.Group(gameRoom).SendAsync("GameStatus", $"{drawingUserOne} och {drawingUserTwo} ritar!");
                await UsersInGame(gameRoom);
                await GameInfo(gameRoom);
            }
            else 
            {
                await Clients.Group(gameRoom).SendAsync("GameCanStart", false);
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
            string word = "";

            if (response.IsSuccessStatusCode)
            {
                string apiResp = await response.Content.ReadAsStringAsync();
                string[] words = JsonConvert.DeserializeObject<string[]>(apiResp) ?? [];
                word = (words?.Length > 0) ? words[0] : "";
                
            }
            currentGame.Rounds[currentGame.Rounds.Count - 1].Word = word;
        }
        public async Task Drawing(Point start, Point end, string color, string gameRoom) 
        {
            await Clients.OthersInGroup(gameRoom).SendAsync("Drawing", start, end, color);
        }

        public async Task SendGuess(string guess)
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                 var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == userConn.GameRoom);

                if (guess == currentGame?.Rounds[currentGame.Rounds.Count -1].Word)
                {
                    userConn.HasGuessedCorrectly = true;
                    await UsersInGame(userConn.GameRoom);
                    await Clients.Caller.SendAsync("ReceiveGuess", guess, userConn.Username);
                    await Clients.OthersInGroup(userConn.GameRoom).SendAsync("ReceiveGuess", "Gissade rätt", userConn.Username);
                } else {
                    await Clients.Group(userConn.GameRoom).SendAsync("ReceiveGuess", guess, userConn.Username);
                }
            }

        }

        public async Task UsersInGame(string gameRoom) 
        {
            var users = _sharedDB.Connection
            .Where(g => g.Value.GameRoom == gameRoom).ToList();

            var activeUSer = users.Find((user) => user.Key == Context.ConnectionId).Value.Username;
            var userValues = users.Select((users) => users.Value);

            try
            {
                await Clients.OthersInGroup(gameRoom).SendAsync("UsersInGame", userValues, "");
                await Clients.Caller.SendAsync("UsersInGame", userValues, activeUSer);
                
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"An error occurred while sending users in game: {ex.Message}");
            }
        }

        public Task GameInfo(string gameRoom) 
        {
            var currentGame = _sharedDB.CreatedGames.FirstOrDefault(exGame => exGame.JoinCode == gameRoom);
            var currentRound = currentGame?.Rounds.Count > 0 ? currentGame?.Rounds[currentGame.Rounds.Count - 1] : new GameRound(); 

            return Clients.Group(gameRoom).SendAsync("receiveGameInfo", currentGame, currentRound);
        }

        public async Task SendClearCanvas (string gameRoom) 
        {
            await Clients.Group(gameRoom).SendAsync("clearCanvas");
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
             if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                _sharedDB.Connection.Remove(Context.ConnectionId, out _);
                Clients.Group(userConn.GameRoom).SendAsync("GameStatus", $"{userConn.Username} har lämnat spelet");

                UsersInGame(userConn.GameRoom);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}