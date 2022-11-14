using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITSupportPortal.Data.Enums;

namespace ITSupportPortal.Models
{
    public class CaseMetric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MetricId { get; set; }
        [ForeignKey("Id")]
        public string CaseId { get; set; }
        public EnumProduct productCategory { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? AssignedTime { get; set; }

        public DateTime? ResolvedTime {get; set; }
        public EnumMetric metricState { get; set; }
    }
}
