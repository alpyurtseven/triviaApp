
using System.ComponentModel.DataAnnotations;

namespace triviaApp.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler birbirleri ile eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}

