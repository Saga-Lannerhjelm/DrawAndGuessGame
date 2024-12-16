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
        public async Task Drawing(Point start, Point end) {
            await Clients.Others.SendAsync("Drawing", start, end);
        }
    }
}