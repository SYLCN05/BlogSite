namespace BlogSite.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public  int BlogId { get; set; }
        public DateTime PublishDate { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public string Message { get; set; }
    }
}
