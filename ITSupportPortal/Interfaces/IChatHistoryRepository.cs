using ITSupportPortal.Models;

namespace ITSupportPortal.Interfaces
{
    public interface IChatHistoryRepository
    {
        ChatHistory AddMessage(string case_id, string username, string message);
        IEnumerable<ChatHistory> GetAllMessages(string case_id);
    }
}