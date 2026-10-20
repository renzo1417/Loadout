using System.ComponentModel.DataAnnotations;

namespace MidtermExam.Models;

public sealed class LoginModel
{
    [Required(ErrorMessage = "Enter your email or call-sign.")]
    public string Identity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterModel
{
    [Required(ErrorMessage = "Enter a display name.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Use between 2 and 30 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Create a password.")]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Use at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class UserAccount
{
    public UserProfile Profile { get; set; }
    public string Password { get; set; }

    public UserAccount(UserProfile profile, string password)
    {
        Profile = profile;
        Password = password;
    }
}

public sealed class RouletteResult
{
    public int RecommendedIndex { get; set; }
    public GameItem? Game { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double MatchScore { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
