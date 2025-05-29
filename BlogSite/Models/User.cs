using Microsoft.EntityFrameworkCore;

namespace BlogSite.Models
{
    public class User
    {
       
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<Blog> Blogs { get; set; }
       
    }
}
