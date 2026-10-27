using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using MidtermExam.Models;
namespace MidtermExam.Services;

// Intentionally process-local for the midterm. Accounts and passwords never go to disk.
public sealed class AccountStore
{
    private readonly ConcurrentDictionary<string, UserAccount> _accounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, UserAccount> _sessions = new();
    private readonly PasswordHasher<UserAccount> _hasher = new();
    private readonly UserAccount _dummy = CreateDummy();
    public UserAccount? Register(RegisterModel model)
    {
        var profile = new UserProfile(model.DisplayName.Trim(), model.Email.Trim(), model.DisplayName.Trim().Replace(' ', '-').ToUpperInvariant());
        var account = new UserAccount(profile);
        account.PasswordHash = _hasher.HashPassword(account, model.Password);
        return _accounts.TryAdd(profile.Email, account) ? account : null;
    }
    public UserAccount? Verify(string email, string password)
    {
        var account = _accounts.GetValueOrDefault(email.Trim());
        var candidate = account ?? _dummy;
        var result = _hasher.VerifyHashedPassword(candidate, candidate.PasswordHash, password);
        return account != null && result != PasswordVerificationResult.Failed ? account : null;
    }
    private static UserAccount CreateDummy()
    {
        var account = new UserAccount(new UserProfile());
        account.PasswordHash = new PasswordHasher<UserAccount>().HashPassword(account, Guid.NewGuid().ToString());
        return account;
    }
    public UserAccount CreateDemo()
    {
        var account = new UserAccount(new UserProfile("Sample player", "", "SAMPLE")) { IsDemo = true };
        var samples = SampleGames.Create();
        for (var i = 0; i < 3; i++) account.Loadout.Slots[i] = samples[i];
        return account;
    }
    public string StartSession(UserAccount account)
    {
        var id = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        _sessions[id] = account;
        return id;
    }
    public UserAccount? FindSession(string? id) => id != null ? _sessions.GetValueOrDefault(id) : null;
    public void EndSession(string? id) { if (id != null) _sessions.TryRemove(id, out _); }
}
