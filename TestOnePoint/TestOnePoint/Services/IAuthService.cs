using System.Threading.Tasks;

namespace TestOnePoint.Services
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string email, string password);
    }
}