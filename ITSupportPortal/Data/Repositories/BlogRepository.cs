using ITSupportPortal.Data.Enums;
using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;

namespace ITSupportPortal.Data.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<BlogDetail> GetAllBlogsforRole(string role)
        {

            if (role == "Admin")
            {
                return _context.BlogDetail
                               .OrderByDescending(c => c.CreatedDate)
                               .ToList();
            }
            if (role == "SupportAgent")
            {
                return _context.BlogDetail
                         .Where(c => c.level == BlogAuthorization.Team || c.level == BlogAuthorization.Public)
                         .OrderByDescending(c => c.CreatedDate)
                         .ToList();
            }
            return _context.BlogDetail
                          .Where(c => c.level == BlogAuthorization.Public)
                          .OrderByDescending(c => c.CreatedDate)
                          .ToList();
        }

        public bool Createblog(string title, string contents, BlogAuthorization ba, EnumProduct ep)
        {
            var blogDetail = new BlogDetail
            {
                Title = title,
                Description = contents,
                CreatedDate = DateTime.Now,
                level = ba,
                ProductCategory = ep,
            };

            _context.BlogDetail.Add(blogDetail);
            var updated = _context.SaveChanges();
            if (updated != 1) { return false; }
            return true;

        }


        public IEnumerable<BlogComment> GetAllCommentsforBlog(string id)
        {
            return _context.BlogComment
                           .Where(c => c.Id == id)
                           .OrderBy(c => c.CommentTime)
                           .ToList();
        }

        public BlogComment AddComment(string id, string message, string username)
        {
            var blogComment = new BlogComment
            {
                Id = id,
                Message = message,
                Username = username,
                CommentTime = DateTime.Now
            };

            _context.BlogComment.Add(blogComment);
            _context.SaveChanges();

            return blogComment;
        }
    }
}
