using Castle.Core.Internal;
using Castle.Core.Logging;
using FakeItEasy;
using FluentAssertions;
using ITSupportPortal.Controllers;
using ITSupportPortal.Data;
using ITSupportPortal.Data.Enums;
using ITSupportPortal.Data.Repositories;
using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;
using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITSupportPortal.Tests.Controller
{
    public class BlogControllerTests
    {
        private readonly IBlogRepository _blogRepository;
        private readonly ILogger<BlogController> _logger;
        public BlogControllerTests()
        {
            _logger = A.Fake<ILogger<BlogController>>();
            _blogRepository = A.Fake<IBlogRepository>();
        }

        //Database Context acts as actual in-memory database instance
        public async Task<ApplicationDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();

            //Populate BlogDetail
            if (await databaseContext.BlogDetail.CountAsync() <= 0)
            {

                databaseContext.BlogDetail.Add(new Models.BlogDetail()
                {
                    Id="blog_1",
                    Title="Template blog title",
                    Description= "Template blog Description",
                    ProductCategory=Data.Enums.EnumProduct.ContentManagementSystem,
                    CreatedDate=DateTime.Now,
                    level=Data.Enums.BlogAuthorization.Public
                });
                databaseContext.BlogDetail.Add(new Models.BlogDetail()
                {
                    Id = "blog_2",
                    Title = "Template blog title",
                    Description = "Template blog Description",
                    ProductCategory = Data.Enums.EnumProduct.WebDriver,
                    CreatedDate = DateTime.Now,
                    level = Data.Enums.BlogAuthorization.Team
                });
                databaseContext.BlogDetail.Add(new Models.BlogDetail()
                {
                    Id = "blog_3",
                    Title = "Template blog title",
                    Description = "Template blog Description",
                    ProductCategory = Data.Enums.EnumProduct.DNSToolbox,
                    CreatedDate = DateTime.Now,
                    level = Data.Enums.BlogAuthorization.Confidential
                });

                await databaseContext.SaveChangesAsync();

            }
            return databaseContext;
        }

        [Fact]
        public async void BlogController_Index_Return_blogs_according_to_role()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
          
            var userManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            userManager.Setup(mock => mock.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser
            {
                UserName = new string(user.Identity.Name)
            });

            userManager.Setup(mock => mock.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string>
            {
                "Customer"
            });

            //Act
            var result = await blogController.Index() as ViewResult;
            var items = result.ViewData.Model as IEnumerable<BlogDetail>;

            //Assert
            items.Should().HaveCount(1);
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async void BlogController_Create_blog_success()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
   
            
            CreateBlogViewModel cv = new CreateBlogViewModel();
            cv.Title = "Test Blog Title";
            cv.Description = "Test Blog Description";
            cv.ProductCategory = Data.Enums.EnumProduct.ContentManagementSystem;
            cv.level = Data.Enums.BlogAuthorization.Team;

            //Act
            var result = await blogController.Create(cv) as RedirectToActionResult;
            var blogCreated = context.BlogDetail.Where(b => b.Title == "Test Blog Title");

            //Assert
            blogCreated.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
        }

        [Fact]
        public async void BlogController_Create_blog_failure()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(),_blogRepository);

            CreateBlogViewModel cv = new CreateBlogViewModel();
            cv.Title = "Test Blog Title";
            cv.Description = "Test Blog Description";
            cv.ProductCategory = Data.Enums.EnumProduct.ContentManagementSystem;
            cv.level = Data.Enums.BlogAuthorization.Team;
            A.CallTo(() => _blogRepository.Createblog(cv.Title, cv.Description, cv.level, cv.ProductCategory)).Returns(false);
            //Act
            var result = await blogController.Create(cv) as ViewResult;

            //Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async void BlogController_Read_blog_id_is_null()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), _blogRepository);

            //Act
            var result = await blogController.Read(null) as NotFoundResult;

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void BlogController_Read_blog_does_not_exist()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), _blogRepository);

            //Act
            var result = await blogController.Read("blog_0") as NotFoundResult;

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void BlogController_Read_blog_customer_not_allowed_to_view_confidential_blog()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            //Act
            var result = await blogController.Read("blog_3") as RedirectToActionResult;

            //Assert
            result.Should().BeOfType<RedirectToActionResult>("Index");

        }

        [Fact]
        public async void BlogController_Read_blog_agent_not_allowed_to_view_confidential_blog()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "agent1"),
                                        new Claim(ClaimTypes.Name, "agent1"),
                                        new Claim(ClaimTypes.Email,"agent1@ITSupport.com"),
                                        new Claim(ClaimTypes.Role,"SupportAgent"),
                                        }, "TestAuthentication"));
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            //Act
            var result = await blogController.Read("blog_3") as RedirectToActionResult;

            //Assert
            result.Should().BeOfType<RedirectToActionResult>("Index");

        }

        [Fact]
        public async void BlogController_Read_blog_customer_not_allowed_to_view_team_blogs()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            //Act
            var result = await blogController.Read("blog_2") as RedirectToActionResult;

            //Assert
            result.Should().BeOfType<RedirectToActionResult>("Index");

        }

        [Fact]
        //Normal Case only tested for customer. Similar for other users.
        public async void BlogController_Read_blog_customer_allowed_to_view_public_blogs()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            //Act
            var result = await blogController.Read("blog_1") as ViewResult;
            var model = result.ViewData.Model as CommentBlogViewModel;

            //Assert
            model.Should().NotBeNull();
            model.blogDetail.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

        }

        [Fact]
        public async void BlogController_Comment_on_blog_failed_on_backend()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), _blogRepository);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            CommentBlogViewModel cbv = new CommentBlogViewModel();
            cbv.blogDetail = new BlogDetail();
            cbv.blogDetail.Id = "blog_1";
            cbv.newComment = new BlogComment();
            cbv.newComment.Message = "New Comment";

            BlogComment blc = new BlogComment();

            A.CallTo(() => _blogRepository.AddComment(It.IsAny<string>(), It.IsAny<string>(), user.Identity.Name)).Returns(blc);

            //Act
            var result = await blogController.Comment(cbv) as ViewResult;
            var message = result.ViewData["Message"];

            //Assert
            message.Should().Be("Comment Not Created. Please try again.");
            result.Should().BeOfType<ViewResult>();

        }

        [Fact]
        public async void BlogController_Comment_on_blog_failed_on_frontend()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), _blogRepository);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            CommentBlogViewModel cbv = new CommentBlogViewModel();
            cbv.blogDetail = new BlogDetail();
            cbv.newComment = new BlogComment();

            BlogComment blc = new BlogComment();

            A.CallTo(() => _blogRepository.AddComment(It.IsAny<string>(), It.IsAny<string>(), user.Identity.Name)).Returns(blc);

            //Act
            var result = await blogController.Comment(cbv) as ViewResult;
            var message = result.ViewData["Message"];

            //Assert
            message.Should().Be("Comment Not Created. Please try again.");
            result.Should().BeOfType<ViewResult>();

        }

        [Fact]
        public async void BlogController_Comment_on_blog_success()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "customer1"),
                                        new Claim(ClaimTypes.Name, "customer1"),
                                        new Claim(ClaimTypes.Email,"customer1@xyz.com"),
                                        new Claim(ClaimTypes.Role,"Customer"),
                                        }, "TestAuthentication"));
            blogController.ControllerContext = new ControllerContext();
            blogController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            CommentBlogViewModel cbv = new CommentBlogViewModel();
            cbv.blogDetail = new BlogDetail();
            cbv.blogDetail.Id = "blog_1";
            cbv.newComment = new BlogComment();
            cbv.newComment.Message = "New Comment";

            //Act
            var result = await blogController.Comment(cbv) as RedirectToActionResult;
            var commentAdded = context.BlogComment.Where(c => c.Id == cbv.blogDetail.Id && c.Username == user.Identity.Name && c.CommentTime == DateTime.Now);

            //Assert
            commentAdded.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();

        }

        [Fact]
        public async void BlogController_Update_model_not_valid()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            EditBlogViewModel ebv = new EditBlogViewModel();
            
            //Act
            var result = await blogController.Update(ebv) as ViewResult;

            //Assert
            result.Should().BeOfType<ViewResult>("Update");

        }

        [Fact]
        public async void BlogController_Update_blog_does_not_exist()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            EditBlogViewModel ebv = new EditBlogViewModel();
            ebv.Id = "blog_5";
            
            //Act
            var result = await blogController.Update(ebv) as NotFoundResult;

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void BlogController_Update_blog()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);
            EditBlogViewModel ebv = new EditBlogViewModel();
            ebv.Id = "blog_3";
            ebv.Description = "Updated Description";

            //Act
            var result = await blogController.Update(ebv) as RedirectToActionResult;
            var updatedBlogDetail = await context.BlogDetail.FirstOrDefaultAsync(b => b.Id == ebv.Id);

            //Assert
            updatedBlogDetail.Description.Should().Be("Updated Description");
            result.Should().BeOfType<RedirectToActionResult>("Index");

        }

        [Fact]
        public async void BlogController_Delete_blog_id_is_null()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);

            //Act
            var result = await blogController.Delete(null) as NotFoundResult;

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void BlogController_Delete_blog_does_not_exist()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);

            //Act
            var result = await blogController.Delete("blog_4") as NotFoundResult;

            //Assert
            result.Should().BeOfType<NotFoundResult>();

        }

        [Fact]
        public async void BlogController_Delete_blog_deleted()
        {
            //Arrange
            var context = await GetDatabaseContext();
            var blogRepository = new BlogRepository(context);
            var blogController = new BlogController(_logger,context, A.Fake<UserManager<IdentityUser>>(), blogRepository);

            //Act
            var result = await blogController.Delete("blog_3") as RedirectToActionResult;
            var blogDeleted = await context.BlogDetail.FindAsync("blog_3");

            //Assert
            blogDeleted.Should().Be(null);
            result.Should().BeOfType<RedirectToActionResult>(nameof(Index));

        }
    }
}
