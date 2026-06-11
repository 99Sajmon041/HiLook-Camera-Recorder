using CameraRecorder.Web.Settings;
using CameraRecorder.Web.ViewModels.Monitoring;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace CameraRecorder.Web.Services.MonitoringService;

public class MonitoringService(IOptions<RecordingStorageSettings> options) : IMonitoringService
{
    private readonly RecordingStorageSettings settings = options.Value;
    public MonitoringTimelineViewModel GetTimeline(DateOnly selectedDate)
    {
        var model = new MonitoringTimelineViewModel
        {
            SelectedDate = selectedDate
        };

        if (!Directory.Exists(settings.RecordingsFolder))
        {
            return model;
        }

        var selectedDateTime = selectedDate.ToDateTime(TimeOnly.MinValue);

        var records = Directory
            .GetFiles(settings.RecordingsFolder, "*.mp4")
            .Select(file => new FileInfo(file))
            .Select(file => new
            {
                File = file,
                StartTime = GetStartTimeOfRecord(file.Name)
            })
            .Where(x => x.StartTime.Date == selectedDateTime.Date)
            .OrderBy(x => x.StartTime)
            .Select(x => new RecordingSegmentViewModel
            {
                FileName = x.File.Name,
                StartTime = x.StartTime,
                EndTime = x.StartTime.AddMinutes(10),
                SizeBytes = x.File.Length
            })
            .ToList();

        model.Segments = records;

        return model;
    }
    private DateTime GetStartTimeOfRecord(string fileName)
    {
        string timestamp = Path.GetFileNameWithoutExtension(fileName)
            .Replace("camera_", "");

        return DateTime.ParseExact(timestamp, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
    }
}
