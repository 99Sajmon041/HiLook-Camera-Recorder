using CameraRecorder.Web.Services.Account;
using CameraRecorder.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CameraRecorder.Web.Controllers;

public class AccountController(IAccountService accountService) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Monitoring");
        }

        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Monitoring");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var loginResult = await accountService.LoginAsync(model);
        if (!loginResult)
        {
            ModelState.AddModelError(string.Empty, "Neplatné přihlašovací údaje.");
            return View(model);
        }

        return RedirectToAction("Index", "Monitoring");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue("Id") ?? string.Empty;

        await accountService.LogoutAsync(userId);

        return RedirectToAction(nameof(Login));
    }
}