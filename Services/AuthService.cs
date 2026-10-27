using Microsoft.AspNetCore.Components.Authorization;
using MidtermExam.Models;

namespace MidtermExam.Services;

// Scoped to a Blazor circuit; identity resolves through the live server session.
public sealed class AuthService(AuthenticationStateProvider authenticationState, AccountStore accounts) : IAuthService
{
    private string? _sessionId;
    public bool IsAuthenticated => accounts.FindSession(_sessionId) != null;
    public UserAccount Account => accounts.FindSession(_sessionId)
        ?? throw new UnauthorizedAccessException("Your session ended. Log in again to continue.");
    public UserProfile CurrentUser => Account.Profile;
    public async Task InitializeAsync()
    {
        var state = await authenticationState.GetAuthenticationStateAsync();
        _sessionId = state.User.FindFirst("session_id")?.Value;
    }
}
