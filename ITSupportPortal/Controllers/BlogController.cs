using ITSupportPortal.Data;
using ITSupportPortal.Data.Enums;
using ITSupportPortal.Data.Repositories;
using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;
using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Data;

namespace ITSupportPortal.Controllers
{
    public class BlogController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IBlogRepository _blogRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<BlogController> _logger;
        public BlogController(ILogger<BlogController> logger,ApplicationDbContext context,UserManager<IdentityUser> userManager, IBlogRepository blogRepository)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _blogRepository = blogRepository;
        }

        // GET: Blog
        [HttpGet]
        [Authorize(Roles = "Customer,SupportAgent,Admin")]
        public async Task<IActionResult> Index()
        {

           var user = await _userManager.FindByNameAsync(User.Identity.Name);
           if(user == null)
            {
                return NotFound();
            }
            var role = await _userManager.GetRolesAsync(user);
            if(role == null) {
                return NotFound();
            }

            return View(_blogRepository.GetAllBlogsforRole(role[0]));
        }

        // GET: Create Blog
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            return View();

        }

        // POST: Create Blog
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateBlogViewModel cvb)
        {
            if (ModelState.IsValid)
            {


                bool success = _blogRepository.Createblog(cvb.Title, cvb.Description, cvb.level, cvb.ProductCategory);
                if (!success)
                {
                    ViewBag.Message = "Blog Not Created. Please try again.";
                    return View();
                }
                _logger.LogInformation("Blog: {title} created successfully",cvb.Title);
                return RedirectToAction("Index");
            }
            else
            {       
                ModelState.AddModelError(string.Empty, "Please fill all the details");
                return View(cvb);
            }
        }

        // GET: Read Blog
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Read(string? Id)
        {
            if(Id == null)
            {
                return NotFound();
            }

            //Get blog Detail
            var blogDetail = await _context.BlogDetail.FindAsync(Id);
            if (blogDetail == null)
            {
                return NotFound();
            }
            else
            {
                //Verify if user is still allowed to view blog
                var authLevel = blogDetail.level;
                if((authLevel == BlogAuthorization.Confidential) && (User.IsInRole("SupportAgent") || User.IsInRole("Customer"))){
                    return RedirectToAction("Index");
                }
                if ((authLevel == BlogAuthorization.Team) && User.IsInRole("Customer")){
                    return RedirectToAction("Index");
                }

            }
            
            //Get All previous Comments
            var previousComments = _blogRepository.GetAllCommentsforBlog(Id);
     
            var commentViewModel = new CommentBlogViewModel
            {
                blogDetail = blogDetail,
                comments=previousComments
            };
            return View(commentViewModel);

        }

        // POST: Comment on Blog
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Comment(CommentBlogViewModel cbv)
        {
            if (cbv.newComment.Message != null && cbv.blogDetail.Id !=null)
            {
                var blogComment = _blogRepository.AddComment(cbv.blogDetail.Id, cbv.newComment.Message, User.Identity.Name);
                if (blogComment.Id == null || blogComment.Username == null || blogComment.Message == null)
                {
                    ViewBag.Message = "Comment Not Created. Please try again.";
                    return View();
                }
                _logger.LogInformation("Comment added on blogID : {blogID} by {user} at {time}",blogComment.Id,blogComment.Username,blogComment.CommentTime);
            }
            else
            {
                ViewBag.Message = "Comment Not Created. Please try again.";
                _logger.LogError("Comment not created.");
                return View();
            }

            return RedirectToAction("Read","Blog",new { Id = cbv.blogDetail.Id });
        }

        // GET: Update Blog
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var blogDetail = await _context.BlogDetail.AsNoTracking().FirstOrDefaultAsync(m=>m.Id == id);
            if (blogDetail == null)
            {
                return NotFound();
            }

            var EditBlogViewModel = new EditBlogViewModel
            {
                Id=id,
                Title = blogDetail.Title,
                Description = blogDetail.Description,
                level = blogDetail.level,
                ProductCategory = blogDetail.ProductCategory,
                CreatedDate = blogDetail.CreatedDate
            };
            
            return View(EditBlogViewModel);
        }

        //POST: Update Blog
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(EditBlogViewModel ebv)
        {

            if (ModelState.IsValid)
            {
                if (ebv.Id == null)
                {
                    ModelState.AddModelError("", "Failed to Update Blog");
                    return View("Update", ebv);
                }
                var blogToUpdate = await _context.BlogDetail.FirstOrDefaultAsync(b => b.Id == ebv.Id);
                if (blogToUpdate == null)
                {
                    return NotFound();
                }
                blogToUpdate.Title = ebv.Title;
                blogToUpdate.Description = ebv.Description;
                blogToUpdate.level = ebv.level;
                blogToUpdate.ProductCategory = ebv.ProductCategory;
                blogToUpdate.CreatedDate = DateTime.Now;
                _logger.LogInformation("Blog with blogID : {blogID} updated at {time}", blogToUpdate.Id, DateTime.Now);

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Please fill all the details");
                return View(ebv);
            }
            
        }

        // POST: Delete Blog
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var result = await _context.BlogDetail.FindAsync(id);
            if(result == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    _context.BlogDetail.Remove(result);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Blog with blogID : {blogID} deleted at {time}", result.Id, DateTime.Now);

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
        }
    }
}
