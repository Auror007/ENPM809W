using FakeItEasy;
using FluentAssertions;
using ITSupportPortal.Controllers;
using ITSupportPortal.Data;
using ITSupportPortal.Data.Repositories;
using ITSupportPortal.Models;
using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITSupportPortal.Tests.Controller
{
    public class AdminControllerTests
    {
        private readonly ILogger<AdminController> _logger;

        public AdminControllerTests()
        {
            _logger = A.Fake<ILogger<AdminController>>();
        }

        [Fact]
        public async void AdminController_CreateUser_success()
        {
            //Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "admin"),
                                        new Claim(ClaimTypes.Name, "admin"),
                                        new Claim(ClaimTypes.Email,"admin@support.com"),
                                        new Claim(ClaimTypes.Role,"Admin"),
                                        }, "TestAuthentication"));

            var userManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var signInManager = new Mock<SignInManager<IdentityUser>>(userManager.Object,Mock.Of<IHttpContextAccessor>(),Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),null,null,null,null); 
            var roleManager = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(),null,null,null,null);

            var adminController = new AdminController(_logger ,userManager.Object, signInManager.Object, roleManager.Object);
            adminController.ControllerContext = new ControllerContext();
            adminController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            userManager.Setup(mock => mock.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);

            userManager.Setup(mock => mock.CreateAsync( It.IsAny<IdentityUser>() , It.IsAny<string>() )).ReturnsAsync(IdentityResult.Success).Verifiable();

            userManager.Setup(mock => mock.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>() )).ReturnsAsync(IdentityResult.Success).Verifiable();

            roleManager.Setup(mock => mock.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            var adminRegisterViewModel = new AdminRegisterViewModel();
            adminRegisterViewModel.accountType = Data.Enums.EnumRole.SupportAgent;
            adminRegisterViewModel.Username = "agent3";
            adminRegisterViewModel.Email = "agent3@ITSupport.com";
            adminRegisterViewModel.Password = "Securepassword@123";
            adminRegisterViewModel.ConfirmedPassword = "Securepassword@123";

            //Act
            var result = await adminController.CreateUser(adminRegisterViewModel) as ViewResult;

            //Assert
            result.Should().BeOfType<ViewResult>("UserCreated");
        }
    }
}
