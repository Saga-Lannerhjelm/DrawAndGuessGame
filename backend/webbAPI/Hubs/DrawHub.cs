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
        
        public async Task Drawing(Point start, Point end, string color) {
            await Clients.Others.SendAsync("Drawing", start, end, color);
        }
    }
}