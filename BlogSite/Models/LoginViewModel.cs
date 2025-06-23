using System.ComponentModel.DataAnnotations;

namespace BlogSite.Models
{
    public class LoginViewModel
    {
        [Required]
        [MinLength(1) , MaxLength(15)]
        public string  Username { get; set; }

        [Required]
        [MinLength(1)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
