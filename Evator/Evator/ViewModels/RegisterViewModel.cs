using System.ComponentModel.DataAnnotations;

namespace Evator.ViewModels
{
    public class RegisterViewModel
    {
        public byte Role { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required, MaxLength(50)]
        public string Agency { get; set; }

        [Required, MaxLength(50)]
        public string Email { get; set; }

        [Required, MaxLength(50), DataType(DataType.Password)]
        public string Password { get; set; }

        [MaxLength(50), DataType(DataType.Password), Compare("Password", ErrorMessage = "The password does not match")]
        public string ConfirmPassword { get; set; }
    }
}