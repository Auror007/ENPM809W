using ITSupportPortal.Models;
using System.ComponentModel.DataAnnotations;

namespace ITSupportPortal.ViewModels
{
    public class CommentBlogViewModel
    {
        public BlogDetail blogDetail { get; set; }    
        public IEnumerable<BlogComment> comments { get; set; }
        public BlogComment newComment { get; set; }
    }
}
