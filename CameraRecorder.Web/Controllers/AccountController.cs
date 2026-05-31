using Microsoft.AspNetCore.Mvc;

namespace CameraRecorder.Web.Controllers;

public class AccountController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}