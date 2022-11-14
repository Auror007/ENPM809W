using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSupportPortal.Data;
using ITSupportPortal.Data.Enums;
using ITSupportPortal.Models;
using ITSupportPortal.ViewModels;
using ITSupportPortal.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using NuGet.Packaging.Signing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ITSupportPortal.Interfaces;

namespace ITSupportPortal.Controllers
{
    public class CasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICaseRepository _caseRepository;
        private readonly IChatHistoryRepository _chatHistoryRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CasesController> _logger;

        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".pdf", new List<byte[]>
            {
                new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D },
            }
        },

        };

        public CasesController(ILogger<CasesController> logger,ApplicationDbContext context, ICaseRepository caseRepository, IChatHistoryRepository chatHistoryRepository,UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _context = context;
            _caseRepository = caseRepository;
            _chatHistoryRepository = chatHistoryRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Cases or Queue
        [HttpGet]
        [Authorize(Roles = "Customer,SupportAgent")]
        public async Task<IActionResult> Index()
        {
            if(User.IsInRole("Customer")) return View(_caseRepository.GetAllCases(User.Identity.Name ?? "Anonymous"));
            else return View("Queue",_caseRepository.GetAllOpenCases());
        }

       
        [HttpGet]
        [Authorize]
        // GET: Cases/Create
        public IActionResult Create()
        {
            return View();
        }


        [HttpGet]
        [Authorize(Roles ="SupportAgent")]
        // GET: Cases/MyCases -> Get Assigned Cases
        public IActionResult MyCases()
        {
            return View("MyCases", _caseRepository.GetAllAssignedCases(User.Identity.Name));
        }

        // POST: Cases/Create
        [HttpPost]
        [Authorize(Roles ="Customer,SupportAgent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCaseViewModel cv)
        {
            if (ModelState.IsValid)
            {
                bool success = Enum.IsDefined(typeof(EnumProduct), cv.ProductCategory);
                if (!success) return ViewBag.Message="Invalid Product Category.";

                string agent = "";
                string? customer = cv.CustomerID;
                if (User.IsInRole("SupportAgent"))
                {
                     agent = User.Identity.Name;
                     if(customer == null)
                     {
                        ViewBag.Message = "No customer Id provided";
                        return View();
                     }
                    try
                    {
                        //check if customer id is registered or not
                        var userExists= await _userManager.FindByNameAsync(customer);
                        if(userExists == null)
                        {
                            ViewBag.Message = "Invalid Customer Id.";
                            return View();
                        }
                        var userValid = await _userManager.IsInRoleAsync(userExists,"Customer");
                        if (!userValid)
                        {
                            ViewBag.Message = "Invalid Customer Id.";
                            return View();
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                }
                else
                {
                    customer = User.Identity.Name;

                }
                var caseadded = _caseRepository.CreateCase(cv.Title, cv.Description, cv.ProductCategory, customer, agent);

                _logger.LogInformation("Case with {id} created by {user}", caseadded.Id, User.Identity.Name);
                var caseMetric = new CaseMetric
                {
                    CaseId=caseadded.Id,
                    productCategory=caseadded.ProductCategory,
                    CreatedDate=DateTime.Now,
                    AssignedTime = User.IsInRole("SupportAgent") ? DateTime.Now: null ,
                    metricState=EnumMetric.Started
                };
                _context.CaseMetric.Add(caseMetric);
                _context.SaveChanges();

                //check file upload
                if (User.IsInRole("Customer") && cv.File != null)
                {
                    //check file upload by customer
                    if (cv.File.Length > 0)
                    {
                        string[] permittedExtensions = { ".jpg", ".pdf" };
                        var extension = Path.GetExtension(cv.File.FileName);
                        var ext = Path.GetExtension(extension).ToLowerInvariant();
                        if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                        {
                            ViewBag.Message = "Invalid File Extension";
                            return View(cv);
                        }

                        using (var reader = new BinaryReader(cv.File.OpenReadStream()))
                        {
                            var signatures = _fileSignature[extension];
                            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                            bool check_magic = signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));

                            if (!check_magic)
                            {
                                _logger.LogError("Invalid file upload attempt by {user}", User.Identity.Name);
                                ViewBag.Message = "Invalid File. Please upload only .jpg or .pdf files.";
                                return View(cv);
                            }
                        }

                        var path = _webHostEnvironment.ContentRootPath;
                        path = path + "upload\\" + @caseadded.Id + ext;
                        var hash = "";
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            var hasher = HashAlgorithm.Create("SHA256");
                            await cv.File.CopyToAsync(fileStream);
                            hash = BitConverter.ToString(hasher.ComputeHash(fileStream)).Replace("-","");

                        }
                        ViewBag.Message = $"File {@caseadded.Id} Uploaded Successfully";
                        _logger.LogInformation("File upload attempt by {user} successfull. Name of file: {file}", User.Identity.Name,caseadded.Id);

                        var updateFile = _caseRepository.UpdateFileData(@caseadded.Id,hash);

                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: Cases/Open -> View Case detail
        [HttpGet]
        [Authorize(Roles = "Customer,SupportAgent")]
        public async Task<IActionResult> Open(string? id)
        {
            OpenCaseViewModel openCaseViewModel = new OpenCaseViewModel();
            if (id == null)
            {
                return NotFound();
            }
            var case_Fetched = await _context.Case.FindAsync(id);
            if (case_Fetched == null)
            {
                return NotFound();
            }
            var checkCaseOwnerShip = User.IsInRole("Customer") ? case_Fetched.CustomerID != User.Identity.Name : case_Fetched.EmployeeID != User.Identity.Name;
            if(checkCaseOwnerShip)
            {
                return NotFound();
            }
            openCaseViewModel.openCase = case_Fetched;
            openCaseViewModel.chatHistory = _chatHistoryRepository.GetAllMessages(id);

            return View(openCaseViewModel);
        }

        // Updates Employee ID if one doesn't already exists and assign case to self.
        [HttpPost]
        [Authorize(Roles = "SupportAgent")]
        public async Task<IActionResult> Assign(string? id)
        {
            //TODO: 
            // Get current employee id if null, update field to current user.
            if (id == null)
            {
                return NotFound();
            }

            var @case = await _context.Case.FindAsync(id);
            if (@case == null)
            {
                return NotFound();
            }

            var employee_name= User.Identity.Name;
            var result =  _caseRepository.AssignEmployeeToCase(id,employee_name);
            if(result == null)
            {
                ViewBag.Message ="Sorry that case was already taken by someone else.";
            }
            else
            {
                var metricEntry = _context.CaseMetric.Where(m => m.CaseId == @case.Id && m.metricState == EnumMetric.Started).FirstOrDefault();
                metricEntry.AssignedTime = DateTime.Now;
                _context.SaveChanges();
                _logger.LogInformation("Case {id} assigned to agent {agent}.",@case.Id,User.Identity.Name);
                ViewBag.Message = "Successfully Assigned the case.";

            }

            return View("Assign");
        }

        // Changes the state of case to closed.
        [HttpPost]
        [Authorize(Roles = "Customer,SupportAgent")]
        public async Task<IActionResult> Close(string? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var @case = await _context.Case.FindAsync(id);
            if (@case == null)
            {
                return NotFound();
            }
            var checkCaseOwnerShip = User.IsInRole("Customer") ? @case.CustomerID != User.Identity.Name : @case.EmployeeID != User.Identity.Name;
            if (checkCaseOwnerShip)
            {
                return NotFound();
            }
            if (@case.State != CaseState.Closed)
            {
                var result = _caseRepository.CloseCase(id);
                //Add to metric table
                var metricEntry = _context.CaseMetric.Where(m => m.CaseId == result.Id && m.metricState == EnumMetric.Started).FirstOrDefault();
                metricEntry.ResolvedTime = DateTime.Now;
                metricEntry.metricState = EnumMetric.Finished;
                _context.SaveChanges();
                _logger.LogInformation("Case {id} closed by user {}.", @case.Id, User.Identity.Name);
            }


            return RedirectToAction("Index");
        }

        // Changes the state of case to open.
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ReOpen(string? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var @case = await _context.Case.FindAsync(id);
            if (@case == null)
            {
                return NotFound();
            }

            var checkCaseOwnerShip = User.IsInRole("Customer") ? @case.CustomerID != User.Identity.Name : true;
            if (checkCaseOwnerShip)
            {
                return NotFound();
            }
            if(@case.CustomerID != User.Identity.Name || @case.State == CaseState.Open)
            {
                return NotFound();
            }
            var result = _caseRepository.ReOpenCase(id);
            var caseMetric = new CaseMetric
            {
                CaseId = result.Id,
                productCategory = result.ProductCategory,
                CreatedDate = DateTime.Now,
                metricState = EnumMetric.Started
            };
            _context.CaseMetric.Add(caseMetric);
            _context.SaveChanges();
            _logger.LogInformation("Case {id} reopened by user {}.", @case.Id, User.Identity.Name);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Download(string? id)
        {
                   
                var directory = _webHostEnvironment.ContentRootPath + "upload\\";
                string[] file = null;
                
                file = Directory.GetFiles(directory, id + "*");
                
                if(file.Length == 0)
                {
                    return NotFound();

                }

                var filePath = file[0];
                var contentHeader = Path.GetExtension(filePath).ToLowerInvariant();
                var lookupHeader = new Dictionary<string, string>()
                {
                    { ".jpg" ,"image/jpeg" },
                    {".pdf","application/pdf" }
                };
                _logger.LogInformation("{user} downloaded file {filename}", User.Identity.Name, filePath);
                return File(System.IO.File.ReadAllBytes(filePath), lookupHeader[contentHeader], System.IO.Path.GetFileName(filePath));
            
        
        }
    }
}
