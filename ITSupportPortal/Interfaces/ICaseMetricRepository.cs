using ITSupportPortal.ViewModels;

namespace ITSupportPortal.Interfaces
{
    public interface ICaseMetricRepository
    {
        List<CaseMetricViewModel> getMetrics();
    }
}