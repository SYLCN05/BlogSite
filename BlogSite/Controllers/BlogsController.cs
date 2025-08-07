
using BlogSite.Data;
using BlogSite.Models;
using BlogSite.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogSite.Controllers
{
    public class BlogsController : Controller
    {
        private readonly BlogDbContext _context;
      
        public BlogsController(BlogDbContext context)
        {
            _context = context;
           
        }
        public async Task<IActionResult> Index()
        {
            //var blogs = await _context.Blogs.ToListAsync();
            var blogs = await _context.Blogs.Where(b => b.Status == 1).ToListAsync();
            return View(blogs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            blog.ViewCount += 1;
            await _context.SaveChangesAsync();
            var comment = await _context.Comments.Where(c => c.BlogId == id).ToListAsync();
            ViewBag.Comments = comment;
            return View(blog);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(Comment comment)
        {
           
            comment.PublishDate = DateTime.Now;
            comment.Username = User.Identity.Name;
            comment.Email = "testmail";
            await _context.Comments.AddAsync(comment);
            var blog = await _context.Blogs.Where(b => b.Id == comment.BlogId).FirstOrDefaultAsync();
            blog.CommentCount += 1;

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", blog);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View("Login");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> LoginConfirm(LoginViewModel usermodel)
        {
            if (ModelState.IsValid) 
            {
                var user =  await _context.Users.FirstOrDefaultAsync(u => u.Username.Equals(usermodel.Username));

                if(user != null)
                {
                    PasswordHasher hasher = new PasswordHasher();
                    bool userVerify = hasher.Verify(usermodel.Password, user.Password);
                    if (userVerify)
                    {

                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                    };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authproperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authproperties);

                        return RedirectToAction("Index");
                    }
                  
                }
                else
                {
                    TempData["Error"] = "Ongeldige inlog poging probeer het opnieuw";
                    return RedirectToAction("Login",usermodel);
                }

            }

            return View(usermodel);
            
           
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterConfirm(RegisterViewModel RegsiterUsermodel)
        {
            if(ModelState.IsValid)
            {
                if (RegsiterUsermodel.Password.Equals(RegsiterUsermodel.PasswordConfirm))
                {
                    PasswordHasher hasher = new PasswordHasher();
                    var hashedPassword=  hasher.Hash(RegsiterUsermodel.Password);
                  
                    var newUser = new User
                    {
                        Username = RegsiterUsermodel.Username,
                        Password = hashedPassword
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, RegsiterUsermodel.Username),
                        
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Wachtwoord en wachtwoord herhaal komen niet overeen";
                    return View("Register", RegsiterUsermodel);
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact(Contact contact)
        {
            contact.CreatedAt = DateTime.Now;
            await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [Authorize(Policy = "Administrator")]
        public async Task<IActionResult> EditBlog(int id)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            var blogModel = new EditViewModel
            {
                Id = blog.Id,
                Name = blog.Name,
                Description = blog.Description,
                ImageUrl = blog.ImageUrl,
                Tags = blog.Tags,

            };
            return View(blogModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditBlog(EditViewModel model)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == model.Id);

            if(blog == null)
            {
                return NotFound();
            }

            blog.Name = model.Name;
            blog.Description = model.Description;
            blog.ImageUrl = model.ImageUrl;
            blog.Tags = model.Tags;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = blog.Id });
        }

        [HttpPost]
        [Authorize(Policy = "Administrator")]
        public async Task<IActionResult> CreateBlog(CreateBlogViewModel model) 
        {
           var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.Equals(User.Identity.Name));

            var newBlog = new Blog
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                PublishDate = DateTime.Now,
                Tags = model.Tags,
                Status = model.Status,
                User = user,
                UserId = user.Id

            };

           await _context.Blogs.AddAsync(newBlog);
           await _context.SaveChangesAsync();

           return RedirectToAction("Index", new {id = newBlog.Id});


        }

        public IActionResult CreateBlog()
        {
            return View();
        }

        public async Task<IActionResult> Search(string searchString)
        {
            var blogToBeFound = await _context.Blogs.
                Where(b => b.Name.StartsWith(searchString))
                .Where(b => b.Status ==1)
                .ToListAsync();
            return View("Index",blogToBeFound);
        }

        public IActionResult AdminLogin()
        {
            return View();
        }

        public async Task<IActionResult> AdminLoginConfirm(LoginViewModel model)
        {
            if (ModelState.IsValid) 
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.Equals(model.Username));

                PasswordHasher passwordHasher = new PasswordHasher();
                passwordHasher.Verify(model.Password, user.Password);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, "Admin")

                };

                var ClaimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync( CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(ClaimsIdentity), authProperties);
                return RedirectToAction("Index");

            }
            TempData["Error"] = "Er ging iets mis tijdens het verifieren van uw gegevens, probeer het opnieuw";
            return View(model);
        }
        public IActionResult AdminRegister()
        {
            return View();

        }

        public async Task<IActionResult> AdminRegisterConfirm(RegisterViewModel model)
        {
            if (ModelState.IsValid && model.Password.Equals(model.PasswordConfirm)) 
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                PasswordHasher hasher = new PasswordHasher();
                var hashedPassword= hasher.Hash(model.Password);

                 var newAdmin= new User
                {
                    Username = model.Username,
                    Password = hashedPassword
                };
                await _context.Users.AddAsync(newAdmin);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            TempData["Error"] = "Er ging iets mis met het maken van een nieuwe account controleer of de ingevulde gegevens kloppen";
            return RedirectToAction("AdminRegister", model);
        }

        
        [Authorize(Policy = "Administrator")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blogToBeDeleted = await _context.Blogs.FindAsync(id);
            if(blogToBeDeleted != null)
            {
                 _context.Blogs.Remove(blogToBeDeleted);
                 await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            TempData["Error"] = "er ging iets mis bij het verwijderen van de blog probeer het opnieuw";
            return RedirectToAction("Index");
        }
        
    }
}
