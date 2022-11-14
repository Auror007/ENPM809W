using ITSupportPortal.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ITSupportPortal.ViewModels
{
    public class EditBlogViewModel
    {
        [Required]
        public string Id { get; set; }
        
        [Display(Name = "Subject of the blog")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Contents")]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Set visibility of the blog:")]
        public BlogAuthorization level { get; set; }

        [Required]
        [Display(Name = "Select Product Category")]
        public EnumProduct ProductCategory { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
