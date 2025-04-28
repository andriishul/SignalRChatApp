using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRChatApp.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChatApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<IdentityUser> userManager;

        public ChatHub(UserManager<IdentityUser> userManager) => this.userManager = userManager;

        public async Task SendTo(string email, ChatContent content)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new HubException("User not found.");
            }

            var senderUsername = GetUsername();

            if (user.UserName != senderUsername)
            {
                await Clients.Caller.SendAsync("ReceivePrivateMessage", senderUsername, content);
            }

            await Clients.User(user.Id).SendAsync("ReceivePrivateMessage", senderUsername, content);
        }

        public async Task JoinGroup(string groupName)
        {        
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync(
              "ReceiveSystemNotification",
              $"{GetUsername()} has entered the group '{groupName}'."
            );
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync(
                "ReceiveSystemNotification",
                $"{GetUsername()} has left the group '{groupName}'."
            );
        }

        public async Task SendToGroup(string groupName, ChatContent content) 
            => await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", GetUsername(), content);

        private string GetUsername() => Context.User?.Identity?.Name ?? "Anonymous";
    }
}
