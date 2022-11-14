using ITSupportPortal.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ITSupportPortal.ViewModels
{
    public class CreateBlogViewModel
    {
        [Display(Name = "Subject of the blog")]
        [Required]

        public string Title { get; set; }

        [Display(Name = "Contents")]
        [Required]

        public string Description { get; set; }

        [Display(Name = "Set visibility of the blog:")]
        [Required]

        public BlogAuthorization level { get; set; }
        [Display(Name = "Select Product Category")]
        [Required]
        public EnumProduct ProductCategory { get; set; }
        public DateTime CreatedDate { get; }
    }
}
