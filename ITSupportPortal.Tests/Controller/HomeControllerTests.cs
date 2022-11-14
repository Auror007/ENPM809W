using FakeItEasy;
using FluentAssertions;
using ITSupportPortal.Controllers;
using ITSupportPortal.Data.Repositories;
using ITSupportPortal.Interfaces;
using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITSupportPortal.Tests.Controller
{
    public class HomeControllerTests
    {
        private readonly ILogger<HomeController> _logger;
        private ICaseMetricRepository _metricRepository;
        private HomeController _homeController;
        public HomeControllerTests()
        {
            _logger = A.Fake<ILogger<HomeController>>();
            _metricRepository = A.Fake<ICaseMetricRepository>();
            _homeController = new HomeController(_logger, _metricRepository);
        }

        [Fact]
        public void HomeController_Index_ReturnsMetrics_for_Adminuser()
        {
            //Arrange
            var listCaseViewModel = A.Fake<List<CaseMetricViewModel>>();
            A.CallTo(() => _metricRepository.getMetrics()).Returns(listCaseViewModel);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "Admin1"),
                                        new Claim(ClaimTypes.Name, "admin1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"Admin"),
                                        }, "TestAuthentication"));
            _homeController.ControllerContext = new ControllerContext();
            _homeController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            //Act
            var result = _homeController.Index();

            //Assert
            result.Should().BeOfType<ViewResult>();

        }

        [Fact]
        public void HomeController_Index_ReturnsMetrics_for_other_authorizedusers()
        {
            //Arrange
            var listCaseViewModel = A.Fake<List<CaseMetricViewModel>>();
            A.CallTo(() => _metricRepository.getMetrics()).Returns(null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            _homeController.ControllerContext = new ControllerContext();
            _homeController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            //Act
            var result = _homeController.Index();

            //Assert
            result.Should().BeOfType<ViewResult>();

        }

    }
}
