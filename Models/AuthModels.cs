using System.ComponentModel.DataAnnotations;
namespace MidtermExam.Models;
public sealed class LoginModel
{
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Enter your password.")]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
public sealed class RegisterModel
{
    [Required(ErrorMessage = "Enter a display name.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Use between 2 and 30 characters.")]
    public string DisplayName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Create a password.")]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Use between 6 and 128 characters.")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
public sealed class UserAccount(UserProfile profile)
{
    public UserProfile Profile { get; } = profile;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsDemo { get; init; }
    public LoadoutState Loadout { get; } = new();
}
public sealed class LoadoutState
{
    public object Sync { get; } = new();
    public GameItem?[] Slots { get; } = new GameItem?[3];
    public List<GameItem> Completed { get; } = new();
    public List<GameItem> Dropped { get; } = new();
    public int? ChosenSlotIndex { get; set; }
    public int TimeAvailable { get; set; } = 45;
    public int BrainBattery { get; set; } = 35;
}
public sealed class RouletteResult
{
    public int RecommendedIndex { get; set; } = -1;
    public GameItem? Game { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double MatchScore { get; set; }
}
