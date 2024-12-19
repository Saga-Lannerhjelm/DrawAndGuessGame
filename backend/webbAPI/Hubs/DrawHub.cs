using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
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

            await Groups.AddToGroupAsync(Context.ConnectionId, userConn.GameRoom);
            _sharedDB.Connection[Context.ConnectionId] = userConn;


            await Clients.OthersInGroup(userConn.GameRoom).SendAsync("GameStatus", $"{userConn.Username} anslöt till spelet");
            await Clients.Caller.SendAsync("GameStatus", $"Välkommen till spelet. Anslutningkoden är {userConn.GameRoom}");

            await UsersInGame(userConn.GameRoom);
        }

        public async Task StartRound(string gameRoom)
        {
            var users = _sharedDB.Connection
            .Where(g => g.Value.GameRoom == gameRoom).ToList();

            int usersInGame = users.Count();

            try
            {
                if (usersInGame >= 3)
            {
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

            // if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? user))
            // {

            // }
        }
        public async Task Drawing(Point start, Point end, string color, string gameRoom) 
        {
            await Clients.OthersInGroup(gameRoom).SendAsync("Drawing", start, end, color);
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