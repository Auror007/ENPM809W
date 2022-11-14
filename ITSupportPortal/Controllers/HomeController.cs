using ITSupportPortal.Data.Repositories;
using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;
using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ITSupportPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICaseMetricRepository _metricRepository;
        public HomeController(ILogger<HomeController> logger,ICaseMetricRepository caseMetricRepository)
        {
            _logger = logger;
            _metricRepository = caseMetricRepository;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(_metricRepository.getMetrics());
            }
            else
            {
                return View();
            }
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}