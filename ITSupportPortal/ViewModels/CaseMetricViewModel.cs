using ITSupportPortal.Data.Enums;

namespace ITSupportPortal.ViewModels
{
    public class CaseMetricViewModel
    {
        public EnumProduct productCategory { get; set; }
        public int numberOfCases { get; set; }
        public string AverageAssignTimeSpan { get; set; }
        public string AverageResolutionTimeSpan {get; set; }
    }
}
