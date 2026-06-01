using Microsoft.AspNetCore.Mvc;

namespace CameraRecorder.Web.Controllers;

public class MonitoringController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
