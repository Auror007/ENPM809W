using Castle.Core.Logging;
using FakeItEasy;
using FluentAssertions;
using ITSupportPortal.Controllers;
using ITSupportPortal.Data;
using ITSupportPortal.Data.Enums;
using ITSupportPortal.Data.Repositories;
using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;
using ITSupportPortal.Tests.Context;
using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITSupportPortal.Tests.Controller
{
    public class CasesControllerTests
    {
        private ApplicationDbContext _context;
        private ICaseRepository _caseRepository;
        private IChatHistoryRepository _chatHistoryRepository;
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CasesController> _logger;
        public CasesControllerTests()
        {
            _logger = A.Fake<ILogger<CasesController>>();
            _caseRepository = A.Fake<ICaseRepository>();
            _chatHistoryRepository = A.Fake<IChatHistoryRepository>();
            _userManager = A.Fake<UserManager<IdentityUser>>();
            _signInManager = A.Fake<SignInManager<IdentityUser>>();
            _webHostEnvironment = A.Fake<IWebHostEnvironment>();
        }

        //Database Context acts as actual inmemory database instance
        public async Task<ApplicationDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();

            //Populate Case table
            if (await databaseContext.Case.CountAsync() <= 0)
            {
                
                    databaseContext.Case.Add(new Models.Case()
                    {
                        Id = "test_string_1",
                        CustomerID = "customer1",
                        EmployeeID = "",
                        Title = "Case 1",
                        Description = "How to add user to AD group? ",
                        State = Data.Enums.CaseState.Open,
                        CreationTime = DateTime.Now,
                        ProductCategory = Data.Enums.EnumProduct.ContentManagementSystem

                    });

                    databaseContext.Case.Add(new Models.Case()
                    {
                        Id = "test_string_2",
                        CustomerID = "customer2",
                        EmployeeID = "agent1",
                        Title = "Case 2",
                        Description = "How to remove user to AD group? ",
                        State = Data.Enums.CaseState.Open,
                        CreationTime = DateTime.Now,
                        ProductCategory = Data.Enums.EnumProduct.WebDriver

                    });
                    databaseContext.Case.Add(new Models.Case()
                    {
                        Id = "test_string_3",
                        CustomerID = "customer2",
                        EmployeeID = "agent2",
                        Title = "Case 3",
                        Description = "How to update user in AD group? ",
                        State = Data.Enums.CaseState.Open,
                        CreationTime = DateTime.Now,

                    });
                    databaseContext.Case.Add(new Models.Case()
                    {
                        Id = "test_string_4",
                        CustomerID = "customer1",
                        EmployeeID = "agent1",
                        Title = "Case 4",
                        Description = "How to upload file to this system ? ",
                        State = Data.Enums.CaseState.Closed,
                        CreationTime = DateTime.Now,
                        ProductCategory = Data.Enums.EnumProduct.ContentManagementSystem

                    });
                await databaseContext.SaveChangesAsync();
                
            }

            //Populate Chathistory table
            if (await databaseContext.ChatHistory.CountAsync() <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    string message = "Message no: " + i;
                    databaseContext.ChatHistory.Add(new Models.ChatHistory()
                    {
                        CaseID = "test_string_1",
                        Username="customer1",
                        CreatedAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(i)),
                        Message = message
                    });
                }
                for (int i = 0; i < 5; i++)
                {
                    string message = "Message no: " + i;
                    databaseContext.ChatHistory.Add(new Models.ChatHistory()
                    {
                        CaseID = "test_string_2",
                        Username = "agent1",
                        CreatedAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(i)),
                        Message = message
                    });
                }
                await databaseContext.SaveChangesAsync();

            }

            //Populate CaseMetricTable
            if (await databaseContext.CaseMetric.CountAsync() <= 0)
            {
               
                databaseContext.CaseMetric.Add(new Models.CaseMetric()
                {
                    MetricId = 1,
                    CaseId = "test_string_1",
                    productCategory = Data.Enums.EnumProduct.ContentManagementSystem,
                    CreatedDate = DateTime.Parse("11 / 6 / 2022 10:38:07 PM"),
                    AssignedTime = null,
                    ResolvedTime = null,
                    metricState = Data.Enums.EnumMetric.Started
                }) ;
                databaseContext.CaseMetric.Add(new Models.CaseMetric()
                {
                    MetricId = 2,
                    CaseId = "test_string_4",
                    productCategory = Data.Enums.EnumProduct.ContentManagementSystem,
                    CreatedDate = DateTime.Parse("11 / 6 / 2022 10:38:08 PM"),
                    AssignedTime = DateTime.Parse("11 / 6 / 2022 10:45:08 PM"),
                    ResolvedTime = DateTime.Parse("11 / 7 / 2022 10:40:08 AM"),
                    metricState = Data.Enums.EnumMetric.Finished
                });


                await databaseContext.SaveChangesAsync();

            }

            //Create Users
            return databaseContext;
        }

        [Fact]
        public async void CasesController_Index_GetCustomerCases()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context,caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            
            var listCases = A.Fake<List<Case>>();

            //Act
            var result =await casesController.Index() as ViewResult;
            listCases = result.ViewData.Model as List<Case>;

            //Assert
            listCases.Should().HaveCount(2);
            result.Should().BeOfType<ViewResult>();

        }

        [Fact]
        public async void CasesController_Index_Get_Cases_InQueue()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            var listCases = A.Fake<IEnumerable<Case>>();


            //Act
            var result = await casesController.Index() as ViewResult;
            listCases = result.ViewData.Model as IEnumerable<Case>;

            //Assert
            listCases.Should().HaveCount(1);
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async void CasesController_MyCases_Get_Assigned_Cases()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            var listCases = A.Fake<List<Case>>();
            A.CallTo(() => _caseRepository.GetAllAssignedCases(user.Identity.Name)).Returns(listCases);


            //Act
            var result = casesController.MyCases();

            //Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async void CasesController_Open_View_Case_Details_Customer()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_1";
           

            //Act
            var result = await casesController.Open(id) as ViewResult;
            model =  result.ViewData.Model as OpenCaseViewModel;

            //Assert
            result.Should().BeOfType<ViewResult>();
            model.openCase.Should().NotBeNull();
            model.openCase.Id.Should().Be(id);
            model.chatHistory.Should().HaveCount(10);
        }

        [Fact]
        public async void CasesController_Open_View_Case_Details_SupportAgent()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_2";


            //Act
            var result = await casesController.Open(id) as ViewResult;
            model = result.ViewData.Model as OpenCaseViewModel;

            //Assert
            result.Should().BeOfType<ViewResult>();
            model.openCase.Should().NotBeNull();
            model.openCase.Id.Should().Be(id);
            model.chatHistory.Should().HaveCount(5);
        }

        [Fact]
        public async void CasesController_Open_View_Case_id_is_null()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = null;


            //Act
            var result = await casesController.Open(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Open_View_Case_not_found()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_does_not_exist";


            //Act
            var result = await casesController.Open(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Open_View_Case_does_not_belong_to_customer()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_2";

            //Act
            var result = await casesController.Open(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Open_View_Case_does_not_belong_to_agent()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_3";

            //Act
            var result = await casesController.Open(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Assign_SupportAgent_assigns_to_self_not_taken_by_other_agent()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_1";
            string verifyMessage = "Successfully Assigned the case.";

            //Act
            var result = await casesController.Assign(id) as ViewResult;
            string viewDataMessage = result.ViewData["Message"] as string;
            var updatedCase = await context.Case.FindAsync(id);
            var updatedMetric = await context.CaseMetric.FindAsync(1);


            //Assert
            updatedCase.EmployeeID.Should().Be(user.Identity.Name);
            updatedMetric.AssignedTime.Should().NotBeNull();
            viewDataMessage.Should().Be(verifyMessage);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();


        }

        [Fact]
        public async void CasesController_Assign_SupportAgent_cannot_assign_to_self_already_taken_by_other_agent()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_3";
            string verifyMessage = "Sorry that case was already taken by someone else.";

            //Act
            var result = await casesController.Assign(id) as ViewResult;
            string viewDataMessage = result.ViewData["Message"] as string;

            //Assert
            viewDataMessage.Should().Be(verifyMessage);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

        }

        [Fact]
        public async void CasesController_Assign_case_not_found()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_456";

            //Act
            var result = await casesController.Assign(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Assign_id_is_null()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = null;

            //Act
            var result = await casesController.Assign(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Close_id_is_null()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = null;

            //Act
            var result = await casesController.Close(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Close_case_not_found()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_456";

            //Act
            var result = await casesController.Close(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Close_Case_does_not_belong_to_customer()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_2";

            //Act
            var result = await casesController.Close(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Close_Case_does_not_belong_to_agent()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_3";

            //Act
            var result = await casesController.Close(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_Close_Case_state_changed_to_closed()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_1";

            //Act
            var metricToTrack = context.CaseMetric.Where(m => m.CaseId == id && m.metricState == EnumMetric.Started).FirstOrDefault();
            var result = await casesController.Close(id) as RedirectToActionResult;
            var updatedCase = await context.Case.FindAsync(id);


            //Assert
            updatedCase.State.Should().Be(Data.Enums.CaseState.Closed);
            metricToTrack.metricState.Should().Be(EnumMetric.Finished);
            metricToTrack.ResolvedTime.Should().NotBeNull();
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>("Index");
        }

        [Fact]
        public async void CasesController_ReOpen_id_is_null()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = null;

            //Act
            var result = await casesController.ReOpen(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_ReOpen_case_not_found()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var casesController = new CasesController(_logger,context, caseRepository, _chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_456";

            //Act
            var result = await casesController.ReOpen(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_ReOpen_Case_does_not_belong_to_customer()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_2";

            //Act
            var result = await casesController.ReOpen(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void CasesController_ReOpen_Case_is_already_in_open_state()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_1";

            //Act
            var result = await casesController.ReOpen(id);

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }


        [Fact]
        public async void CasesController_ReOpen_Case_normal_scenario()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var caseRepository = new CaseRepository(context);
            var chatHistoryRepository = new ChatHistoryRepository(context);

            var casesController = new CasesController(_logger,context, caseRepository, chatHistoryRepository, _userManager, _signInManager, _webHostEnvironment);
            OpenCaseViewModel model = new OpenCaseViewModel();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            casesController.ControllerContext = new ControllerContext();
            casesController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            string id = "test_string_4";

            //Act
            var result = await casesController.ReOpen(id);
            var updatedCase = await context.Case.FindAsync(id);


            //Assert
            updatedCase.State.Should().Be(CaseState.Open);
            updatedCase.EmployeeID.Should().Be(String.Empty);
            result.Should().BeOfType<RedirectToActionResult>("Index");

        }

    }
}
