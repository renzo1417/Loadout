using MidtermExam.Models;

namespace MidtermExam.Services;

public interface IAuthService
{
    UserProfile CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool Login(string identity, string password, out string? error);
    bool Register(RegisterModel model, out string? error);
    void Logout();
    event Action? OnAuthStateChanged;
}
