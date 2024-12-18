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


            await Clients.OthersInGroup(userConn.GameRoom).SendAsync("JoinedGame", $"{userConn.Username} anslöt till spelet");
            await Clients.Caller.SendAsync("JoinedGame", $"Välkommen till spelet. Anslutningkoden är {userConn.GameRoom}");

            await UsersInGame(userConn.GameRoom);
        }

        public async Task StartRound(string gameRoom)
        {
            var users = _sharedDB.Connection
            .Where(g => g.Value.GameRoom == gameRoom).ToList();

            int usersInGame = users.Count();

            if (usersInGame >= 3)
            {
                var rnd = new Random();

                int randomNr1 = rnd.Next(1, usersInGame + 1);
                int randomNr2 = rnd.Next(1, usersInGame + 1);

                while (randomNr2 == randomNr1)
                {
                    randomNr2 = rnd.Next(1, usersInGame + 1);
                }

                var selectedUser1 = users[randomNr1].Value;
                var selectedUser2 = users[randomNr2].Value;

                selectedUser1.IsDrawing = true;
                selectedUser2.IsDrawing = true;

                var drawingUserOne = selectedUser1.Username;
                var drawingUserTwo = selectedUser2.Username;

                await Clients.Group(gameRoom).SendAsync("GameCanStart", true);
                await Clients.Group(gameRoom).SendAsync("GameStatus", $"{drawingUserOne} och {drawingUserTwo} ritar!");
                await UsersInGame(gameRoom);
            }
            else 
            {
                await Clients.Group(gameRoom).SendAsync("GameCanStart", false);
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

            await Clients.OthersInGroup(gameRoom).SendAsync("UsersInGame", userValues, "");
            await Clients.Caller.SendAsync("UsersInGame", userValues, activeUSer);
        }
    }
}