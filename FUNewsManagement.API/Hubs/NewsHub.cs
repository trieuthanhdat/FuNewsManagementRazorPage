using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.API.Hubs
{
    public class NewsHub : Hub
    {
        public async Task SendNewsUpdate(string newsTitle)
        {
            await Clients.All.SendAsync("ReceiveNewsUpdate", newsTitle);
        }
    }

}
