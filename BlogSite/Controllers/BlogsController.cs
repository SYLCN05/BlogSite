
using BlogSite.Data;
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
            var taak = await _context.Blogs.FindAsync(id);
            return View(taak);
        }

        
    }
}
