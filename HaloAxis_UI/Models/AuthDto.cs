namespace HaloAxis_UI.Models
{
    using System.Text.Json.Serialization;

    public record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string Gender
    );

    public record LoginRequest(string Email, string Password);

    public class AuthUser
    {
        public string Id { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Gender { get; set; } = default!;
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

}
