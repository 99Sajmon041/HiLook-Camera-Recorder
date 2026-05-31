using Microsoft.AspNetCore.Mvc;

namespace CameraRecorder.Web.Controllers;

public class MonitoringController : Controller
{
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(ILogger<MonitoringController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}
