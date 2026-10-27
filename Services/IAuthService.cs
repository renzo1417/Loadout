using MidtermExam.Models;
namespace MidtermExam.Services;
public interface IAuthService
{
    UserProfile CurrentUser { get; }
    UserAccount Account { get; }
    bool IsAuthenticated { get; }
    Task InitializeAsync();
}
