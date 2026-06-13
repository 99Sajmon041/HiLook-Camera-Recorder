using CameraRecorder.Web.Settings;
using CameraRecorder.Web.ViewModels.Monitoring;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

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
        model.MotionEvents = LoadMotionEvents(selectedDate);
        model.Slots = CreateTimelineSlots(model);

        return model;
    }

    private DateTime GetStartTimeOfRecord(string fileName)
    {
        string timestamp = Path.GetFileNameWithoutExtension(fileName)
            .Replace("camera_", "");

        return DateTime.ParseExact(timestamp, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
    }

    private List<MotionEventViewModel> LoadMotionEvents(DateOnly selectedDate)
    {
        var result = new List<MotionEventViewModel>();

        if (string.IsNullOrWhiteSpace(settings.MotionEventsFilePath))
        {
            return result;
        }

        if (!File.Exists(settings.MotionEventsFilePath))
        {
            return result;
        }

        var selectedDateTime = selectedDate.ToDateTime(TimeOnly.MinValue);

        foreach (var line in File.ReadLines(settings.MotionEventsFilePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var motionEvent = JsonSerializer.Deserialize<MotionEventViewModel>(line);

            if (motionEvent is null)
            {
                continue;
            }

            if (motionEvent.DetectedAt.Date == selectedDateTime.Date)
            {
                result.Add(motionEvent);
            }
        }

        return result
            .OrderBy(x => x.DetectedAt)
            .ToList();
    }

    private List<TimelineSlotViewModel> CreateTimelineSlots(MonitoringTimelineViewModel model)
    {
        var slots = new List<TimelineSlotViewModel>();
        var dayStart = model.SelectedDate.ToDateTime(TimeOnly.MinValue);

        for (var i = 0; i < 144; i++)
        {
            var slotStart = dayStart.AddMinutes(i * 10);
            var slotEnd = slotStart.AddMinutes(10);

            var segment = model.Segments.FirstOrDefault(x => x.StartTime < slotEnd && x.EndTime > slotStart);

            var hasMotion = model.MotionEvents.Any(x => x.DetectedAt >= slotStart && x.DetectedAt < slotEnd);

            slots.Add(new TimelineSlotViewModel
            {
                StartTime = slotStart,
                EndTime = slotEnd,
                FileName = segment?.FileName,
                HasMotion = hasMotion
            });
        }

        return slots;
    }
}
