using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ITSupportPortal.Models
{
    public class BlogComment
    {
        public string Id { get; set; } 
        public string Username { get; set; }

        [Display(Name = "Enter your Comment:")]
        public string Message { get;set; }
        public DateTime CommentTime { get; set; }
    }
}
