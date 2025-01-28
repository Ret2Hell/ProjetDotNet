using ProjetDotNet.Models;

namespace ProjetDotNet.Service;


public interface IAuthService
{
    Task<string> LoginAsync(LoginViewModel model);
    Task<(bool isSuccess, List<string> errors)> RegisterAsync(RegisterViewModel model);
}
