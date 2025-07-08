using System.ComponentModel.DataAnnotations;

namespace BlogSite.Models
{
    public class EditViewModel
    {
        public int Id { get; set; }
        [Required]
        [MinLength(1), MaxLength(20)]
        public string Name { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        [MinLength(3), MaxLength(20)]
        public string Tags { get; set; }
    }
}
