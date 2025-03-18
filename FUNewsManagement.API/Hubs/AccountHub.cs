using FUNewsManagement.Domain.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace FUNewsManagement.API.Hubs
{
    public class AccountHub : Hub
    {
        public async Task NotifyAccountChange(string message)
        {
            await Clients.All.SendAsync("ReceiveAccountUpdate", message);
        }
        public async Task SendAccountUpdate()
        {
            await Clients.All.SendAsync("ReceiveAccountUpdate");
        }
        public async Task SendAccountCreated(RegisterDTO newUser)
        {
            await Clients.All.SendAsync("ReceiveAccountUpdate", newUser);
        }

        public async Task SendAccountUpdated(RegisterDTO updatedUser)
        {
            await Clients.All.SendAsync("ReceiveAccountUpdated", updatedUser);
        }

        public async Task SendAccountDeleted(int accountID)
        {
            await Clients.All.SendAsync("ReceiveAccountDeleted", accountID);
        }
    }
}
