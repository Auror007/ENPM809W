using ITSupportPortal.Data.Enums;
using ITSupportPortal.Models;

namespace ITSupportPortal.Interfaces
{
    public interface IBlogRepository
    {
        BlogComment AddComment(string id, string message, string username);
        bool Createblog(string title, string contents, BlogAuthorization ba, EnumProduct ep);
        IEnumerable<BlogDetail> GetAllBlogsforRole(string role);
        IEnumerable<BlogComment> GetAllCommentsforBlog(string id);
    }
}