using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using webbAPI.Models;

namespace webbAPI.Hubs
{
    public class DrawHub : Hub
    {

        // public override Task OnConnectedAsync()
        // {
        //     Groups.AddToGroupAsync(Context.ConnectionId, "Test");
        //     Clients.Group("Test").SendAsync("Message", $"{Context.ConnectionId} har anslutit till grupp Test");
        //     return base.OnConnectedAsync();
        // }
        
        public async Task JoinGame (string role, string gameRoom) {

            await Groups.AddToGroupAsync(Context.ConnectionId, gameRoom);
            await Clients.OthersInGroup(gameRoom).SendAsync("JoinedGame", $"{Context.ConnectionId} anslöt som {role}", role);

        }
        public async Task Drawing(Point start, Point end, string color, string gameRoom) {
            await Clients.OthersInGroup(gameRoom).SendAsync("Drawing", start, end, color);
        }
    }
}