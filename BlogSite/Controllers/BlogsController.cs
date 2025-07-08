
using BlogSite.Data;
using BlogSite.Models;
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
        public readonly BlogDbContext _context;
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
            await _context.Comments.AddAsync(comment);
            var blog = await _context.Blogs.Where(b => b.Id == comment.BlogId).FirstOrDefaultAsync();
            blog.CommentCount += 1;

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = comment.BlogId});
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

                if(user != null && usermodel.Password.Equals(user.Password))
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
                else
                {
                    TempData["Error"] = "Ongeldige inlog poging prbeer het opnieuw";
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
        public async Task<IActionResult> RegisterConfirm(User usermodel)
        {
            if (ModelState.IsValid) 
            {
                var newUser = await _context.Users.AddAsync(usermodel);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            TempData["Error"] = "Oei er ging iets mis bij het versturen van de gegevens, controleer of de gegevens die je verstuurd kloppen";
            return View("Register",usermodel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact(Contact contact)
        {
            contact.CreatedAt = DateTime.Now;
            await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
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

       
        
    }
}
