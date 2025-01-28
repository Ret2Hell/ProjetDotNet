using ProjetDotNet.Models;

namespace ProjetDotNet.Service;


public interface IAuthService
{
    Task<string> LoginAsync(LoginViewModel model);
    Task<bool> RegisterAsync(RegisterViewModel model);
}
