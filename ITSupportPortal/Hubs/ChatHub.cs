using ITSupportPortal.Data;
using ITSupportPortal.Data.Repositories;
using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ITSupportPortal.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatHistoryRepository _chatHistoryRepository;
        private readonly ApplicationDbContext _applicationDbContext;

        public ChatHub(IChatHistoryRepository chatHistoryRepository, ApplicationDbContext applicationDbContext)
        {
            _chatHistoryRepository = chatHistoryRepository;
            _applicationDbContext = applicationDbContext;
        }
        public async Task SendMessage(string user, string message,string groupName)
        {
            var chatHistory = new ChatHistory
            {
                CaseID = groupName,
                Username = user,
                CreatedAt = DateTime.Now,
                Message = message,
            };

            _applicationDbContext.ChatHistory.Add(chatHistory);
            _applicationDbContext.SaveChanges();
            var date = DateTime.Now;
            await Clients.Group(groupName).SendAsync("ReceiveMessage", $"  {user}: [{date}] > {message}");

        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

          //  await Clients.Group(groupName).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        //    await Clients.Group(groupName).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has left the group {groupName}.");
        }
    }
}