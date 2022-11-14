using ITSupportPortal.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSupportPortal.Models

{
    public class Case
    {
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? Id { get; set; }
        public string? CustomerID { get; set; }
        public string? EmployeeID { get; set; }
        public string? Title { get; set; }
        public CaseState State { get; set; }
        public DateTime? CreationTime { get; set; }
        public string? Description { get; set; }
        public EnumProduct ProductCategory { get; set; }
        public bool? UploadedFile { get; set; }
        public string? UploadedFileHash { get; set; }   
    }

    
}
