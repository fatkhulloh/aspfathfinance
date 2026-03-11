using System.ComponentModel.DataAnnotations;

namespace ImsMVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username wajib diisi")]
        [Display(Name = "Nama Pengguna")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Konfirmasi password wajib diisi")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password tidak cocok")]
        public string ConfirmPassword { get; set; }
    }
}