using System.Threading.Tasks;

namespace EStoreManagement.Services
{
    public interface IAuthService
    {
        Task<string> GenerateTokenAsync(string username, string password);
        Task<bool> ValidateUserAsync(string username, string password);
    }
}