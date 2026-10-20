using MidtermExam.Models;

namespace MidtermExam.Services;

public sealed class AuthService : IAuthService
{
    private readonly Dictionary<string, UserAccount> _users = new(StringComparer.OrdinalIgnoreCase);
    private UserProfile _currentUser;

    public UserProfile CurrentUser => _currentUser;
    public bool IsAuthenticated { get; private set; } = true;
    public event Action? OnAuthStateChanged;

    public AuthService()
    {
        // Default seed demo user
        var demoProfile = new UserProfile("Operator Renzo", "operator@loadout.gg", "GHOST-01")
        {
            Level = 2,
            CurrentXp = 450,
            ClearedCount = 2,
            DroppedCount = 1
        };
        demoProfile.Badges.Add(new Badge("first_clear", "First Blood", "Cleared your first backlog title.", "🎯", BadgeTier.Bronze));
        demoProfile.Badges.Add(new Badge("tactical_mind", "Tactical Discipline", "Adhered strictly to the Slot Lock protocol.", "⚡", BadgeTier.Silver));

        var defaultAccount = new UserAccount(demoProfile, "password123");
        _users["operator@loadout.gg"] = defaultAccount;
        _users["GHOST-01"] = defaultAccount;

        _currentUser = demoProfile;
    }

    public bool Login(string identity, string password, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(identity))
        {
            error = "Identity is required.";
            return false;
        }

        if (_users.TryGetValue(identity.Trim(), out var account))
        {
            if (account.Password == password)
            {
                _currentUser = account.Profile;
                IsAuthenticated = true;
                OnAuthStateChanged?.Invoke();
                return true;
            }

            error = "Invalid password.";
            return false;
        }

        // Allow demo instant login if user entered any new credentials
        var generatedCallsign = identity.Contains('@')
            ? identity.Split('@')[0].ToUpperInvariant()
            : identity.ToUpperInvariant();

        var newProfile = new UserProfile(identity, identity.Contains('@') ? identity : $"{identity}@loadout.gg", generatedCallsign)
        {
            Level = 1,
            CurrentXp = 100
        };
        var newAccount = new UserAccount(newProfile, password);
        _users[identity.Trim()] = newAccount;
        _users[generatedCallsign] = newAccount;

        _currentUser = newProfile;
        IsAuthenticated = true;
        OnAuthStateChanged?.Invoke();
        return true;
    }

    public bool Register(RegisterModel model, out string? error)
    {
        error = null;
        if (_users.ContainsKey(model.Email.Trim()))
        {
            error = "An account with this email already exists.";
            return false;
        }

        var callsign = model.DisplayName.Replace(" ", "-").ToUpperInvariant();
        var profile = new UserProfile(model.DisplayName, model.Email, callsign)
        {
            Level = 1,
            CurrentXp = 0
        };

        var account = new UserAccount(profile, model.Password);
        _users[model.Email.Trim()] = account;
        _users[callsign] = account;

        _currentUser = profile;
        IsAuthenticated = true;
        OnAuthStateChanged?.Invoke();
        return true;
    }

    public void Logout()
    {
        // Revert to demo default
        if (_users.TryGetValue("operator@loadout.gg", out var defaultAccount))
        {
            _currentUser = defaultAccount.Profile;
        }
        IsAuthenticated = false;
        OnAuthStateChanged?.Invoke();
    }
}
