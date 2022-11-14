using ITSupportPortal.Data.Enums;
using ITSupportPortal.Interfaces;
using ITSupportPortal.ViewModels;
using System;

namespace ITSupportPortal.Data.Repositories
{
    public class CaseMetricRepository : ICaseMetricRepository
    {
        private readonly ApplicationDbContext _context;

        public CaseMetricRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CaseMetricViewModel> getMetrics()
        {
            List<CaseMetricViewModel> result = new List<CaseMetricViewModel>();
            var values = Enum.GetValues(typeof(EnumProduct));
            foreach (EnumProduct item in values)
            {

                var caseMetric = _context.CaseMetric.Where(c => c.productCategory == item && c.metricState == EnumMetric.Finished).ToList();

                if (caseMetric == null || caseMetric.Count == 0)
                {
                    continue;
                }
                List<TimeSpan> ListAssignTime = new List<TimeSpan>();
                List<TimeSpan> ListResolutionTime = new List<TimeSpan>();

                for (var i = 0; i < caseMetric.Count; i++)
                {
                    var metric = caseMetric[i];
                    ListAssignTime.Add((metric.AssignedTime - metric.CreatedDate).GetValueOrDefault());
                    ListResolutionTime.Add((metric.ResolvedTime - metric.AssignedTime).GetValueOrDefault());

                }
                double assignAverage = ListAssignTime.Average(timeSpan => timeSpan.Ticks);
                long assignAverageLong = Convert.ToInt64(assignAverage);
                string assignTimeAverage = new TimeSpan(assignAverageLong).ToString(@"d\:hh\:mm\:ss");

                double resolveAverage = ListResolutionTime.Average(timeSpan => timeSpan.Ticks);
                long resolveAverageLong = Convert.ToInt64(resolveAverage);
                string resolveTimeAverage = new TimeSpan(resolveAverageLong).ToString(@"d\:hh\:mm\:ss");


                var caseMetricViewModel = new CaseMetricViewModel
                {
                    productCategory = item,
                    numberOfCases = caseMetric.Count,
                    AverageAssignTimeSpan = assignTimeAverage,
                    AverageResolutionTimeSpan = resolveTimeAverage
                };
                result.Add(caseMetricViewModel);

            }
            return result;
        }
    }
}
