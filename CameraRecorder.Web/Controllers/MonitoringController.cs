using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CameraRecorder.Web.Controllers;

[Authorize]
public class MonitoringController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Detail()
    {
        return View();
    }

    public IActionResult Delete()
    {
        return View();
    }
}
