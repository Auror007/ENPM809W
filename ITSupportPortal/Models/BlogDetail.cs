using ITSupportPortal.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSupportPortal.Models
{
    public class BlogDetail
    {
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        
        [Required]
        [Display(Name = "Subject of the blog")]

        public string? Title { get; set; }
        [Required]
        [Display(Name = "Contents")]

        public string? Description { get; set; }
        [Required]
        [Display(Name = "Set visibility of the blog:")]

        public BlogAuthorization level { get; set; }
        [Required]
        [Display(Name = "Select Product Category")]

        public EnumProduct ProductCategory { get; set; }

        public DateTime CreatedDate { get; set; }


    }
}
