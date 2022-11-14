using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace ITSupportPortal.Data.Repositories
{
    public class ChatHistoryRepository : IChatHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public ChatHistory AddMessage(string case_id, string username, string message)
        {
            var chatHistory = new ChatHistory
            {
                CaseID = case_id,
                Username = username,
                CreatedAt = DateTime.Now,
                Message = message,
            };

            chatHistory = _context.ChatHistory.Add(chatHistory).Entity;
            _context.SaveChanges();

            return chatHistory;
        }

        public IEnumerable<ChatHistory> GetAllMessages(string case_id)
        {
            return _context.ChatHistory.Where(c => c.CaseID == case_id)
                                            .OrderBy(c => c.CreatedAt)
                                            .ToList();
        }
    }
}
