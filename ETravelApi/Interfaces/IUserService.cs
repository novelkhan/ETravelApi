using ETravelApi.Models;
using System.Threading.Tasks;

namespace ETravelApi.Interfaces
{
    public interface IUserService
    {
        bool IsAuthenticated();
        Task<bool> IsAuthenticatedAsync();

        string? GetCurrentUserId();
        Task<string?> GetCurrentUserIdAsync();

        User? GetCurrentUser();
        Task<User?> GetCurrentUserAsync();
    }
}
