using CameraRecorder.Web.Entities;
using CameraRecorder.Web.ViewModels.Account;
using Microsoft.AspNetCore.Identity;

namespace CameraRecorder.Web.Services.Account;

public sealed class AccountService(UserManager<ApplicationUser> userManager, ILogger<AccountService> logger, SignInManager<ApplicationUser> signInManager) : IAccountService
{
    public async Task<bool> LoginAsync(LoginViewModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            logger.LogInformation("User tries to Log-in but e-mail does not exist. Email: {Email}", model.Email);
            return false;
        }

        var loginResult = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
        if (!loginResult.Succeeded)
        {
            logger.LogInformation("User tries to Log-in but credentials are wrong. Logging e-mail: {Email}", model.Email);
            return false;
        }

        return true;
    }

    public async Task LogoutAsync(string userId)
    {
        logger.LogInformation("User with ID: {UserId} was signed out.", userId);

        await signInManager.SignOutAsync();
    }
}