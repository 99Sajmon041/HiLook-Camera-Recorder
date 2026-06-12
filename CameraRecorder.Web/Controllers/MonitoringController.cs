using CameraRecorder.Web.Services.MonitoringService;
using CameraRecorder.Web.Services.RecordingFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CameraRecorder.Web.Controllers;

[Authorize]
public sealed class MonitoringController(IMonitoringService monitoringService, IRecordingFileService recordingFileService) : Controller
{
    [HttpGet]
    public IActionResult Index(DateOnly? selectedDate)
    {
        var date = selectedDate ?? DateOnly.FromDateTime(DateTime.Today);

        var model = monitoringService.GetTimeline(date);

        return View(model);
    }

    [HttpGet]
    public IActionResult Video(string fileName)
    {
        var filePath = recordingFileService.GetSafeFilePath(fileName);

        return PhysicalFile(filePath, "video/mp4", enableRangeProcessing: true);
    }
}