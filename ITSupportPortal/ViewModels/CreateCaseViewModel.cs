using ITSupportPortal.Data.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ITSupportPortal.ViewModels
{
    public class CreateCaseViewModel
    {
        [Required]
        [Display(Name = "Enter Subject")]
        [DataType(DataType.Text)]
        [StringLength(50)]
        public string? Title { get; set; }

        [Required]
        [Display(Name = "Describe your problem")]
        [DataType(DataType.Text)]
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [Display(Name = "Select a Product Category")]
        public EnumProduct ProductCategory { get; set; }

        [Display(Name = "Customer Id")]
        public string? CustomerID { get; set; }

        [Display(Name = "Upload JPG/PDF File")]
        public IFormFile? File { get; set; }
    }
}
