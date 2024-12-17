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
        public async Task JoinGame (UserConnection userConn) {

            await Groups.AddToGroupAsync(Context.ConnectionId, userConn.GameRoom);
            _sharedDB.Connection[Context.ConnectionId] = userConn;


            await Clients.OthersInGroup(userConn.GameRoom).SendAsync("JoinedGame", $"{userConn.Username} anslöt till spelet");
            await Clients.Caller.SendAsync("JoinedGame", $"Välkommen till spelet. Anslutningkoden är {userConn.GameRoom}");
        }
        public async Task Drawing(Point start, Point end, string color, string gameRoom) {
            await Clients.OthersInGroup(gameRoom).SendAsync("Drawing", start, end, color);
        }
    }
}