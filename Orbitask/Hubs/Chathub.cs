using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Orbitask.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User {userId} connected to ChatHub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User {userId} disconnected from ChatHub");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(int chatId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{chatId}");
            Console.WriteLine($"User {userId} joined chat {chatId}");
        }

        public async Task LeaveChat(int chatId)
        { 
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{chatId}");
            Console.WriteLine($"User {userId} left chat {chatId}");
        }

        public async Task NotifyTyping(int chatId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Clients.OthersInGroup($"chat-{chatId}")
                .SendAsync("UserTyping", new { userId, chatId });
        }
    }
}
