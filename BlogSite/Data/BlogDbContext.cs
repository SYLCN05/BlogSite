using BlogSite.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSite.Data
{
    public class BlogDbContext:DbContext
    {

        public BlogDbContext(DbContextOptions<BlogDbContext> options ):base(options)
        {
            
        }
        public DbSet<Blog> Blogs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }
    }
}
