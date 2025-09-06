using System.ComponentModel.DataAnnotations;
namespace HaloAxis_UI.Models
{
    

    public class LoginViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required] public string FirstName { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, DataType(DataType.Password), MinLength(6)] public string Password { get; set; } = "";
        [Required, Compare(nameof(Password)), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
        [Required] public string Gender { get; set; } = "Other";
        public string? Phone { get; set; }
    }

}
