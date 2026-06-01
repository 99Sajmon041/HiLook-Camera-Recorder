using CameraRecorder.Web.ViewModels.Account;

namespace CameraRecorder.Web.Services.Account;

public interface IAccountService
{
    Task<bool> LoginAsync(LoginViewModel model);
    Task LogoutAsync(string userId);
}