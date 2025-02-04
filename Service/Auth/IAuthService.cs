using ProjetDotNet.Models;

namespace ProjetDotNet.Service;


public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginViewModel model);
    Task<RegisterResponse> RegisterAsync(RegisterViewModel model);
    Task<ConfirmEmailResponse> ConfirmEmailAsync(string userId, string token);
}
