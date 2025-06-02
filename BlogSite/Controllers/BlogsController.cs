
using BlogSite.Data;
using BlogSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost]
        public async Task<IActionResult> LoginConfirm(User usermodel)
        {
           
            var entry = await _context.Users.Where(u => u.Username.Equals(usermodel.Username) && u.Password.Equals(usermodel.Password)).FirstOrDefaultAsync();
            if (entry != null) 
            {
                return RedirectToAction("Index");
            }
            TempData["Error"] = "De opgegeven gebruikersnaam of wachtwoord is niet correct";
            return View("Login", entry);
           
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
        
    }
}
