using CameraRecorder.Web.Services.RecordingFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CameraRecorder.Web.Controllers;

[Authorize]
public sealed class MonitoringController(IRecordingFileService recordingFileService) : Controller
{
    [HttpGet]
    public IActionResult Index(DateOnly? selectedDate)
    {
        var date = selectedDate ?? DateOnly.FromDateTime(DateTime.Today);

        var model = recordingFileService.GetRecordingsByDate(date);

        return View(model);
    }

    [HttpGet]
    public IActionResult Detail(string fileName)
    {
        var model = recordingFileService.GetRecordingDetail(fileName);

        return View(model);
    }

    [HttpGet]
    public IActionResult Video(string fileName)
    {
        var filePath = recordingFileService.GetSafeFilePath(fileName);

        return PhysicalFile(filePath, "video/mp4", enableRangeProcessing: true);
    }

    public IActionResult Delete(string fileName)
    {
        return View();
    }

    public IActionResult Download(string fileName)
    {
        return View();
    }
}