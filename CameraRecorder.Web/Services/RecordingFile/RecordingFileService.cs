using CameraRecorder.Web.Settings;
using CameraRecorder.Web.ViewModels.Monitoring;
using Microsoft.Extensions.Options;

namespace CameraRecorder.Web.Services.RecordingFile;

public sealed class RecordingFileService(IOptions<RecordingStorageSettings> options) : IRecordingFileService
{
    private readonly RecordingStorageSettings settings = options.Value;

    public RecordingListViewModel GetRecordingsByDate(DateOnly selectedDate)
    {
        var model = new RecordingListViewModel
        {
            SelectedDate = selectedDate
        };

        if (!Directory.Exists(settings.RecordingsFolder))
        {
            return model;
        }

        var selectedDatetime = selectedDate.ToDateTime(TimeOnly.MinValue);

        model.Recordings = Directory
            .GetFiles(settings.RecordingsFolder, "*.mp4")
            .Select(file => new FileInfo(file))
            .Where(file => file.CreationTime.Date == selectedDatetime.Date)
            .OrderByDescending(file => file.CreationTime)
            .Select(file => new RecordingListItemViewModel
            {
                FileName = file.Name,
                CreatedAt = file.CreationTime,
                DurationTime = new TimeOnly(0, 10),
                SizeBytes = file.Length,
                ThumbnailPath = null
            })
            .ToList();

        return model;
    }

    public string GetSafeFilePath(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(settings.RecordingsFolder, safeFileName);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Recording was not found.", fullPath);
        }

        return fullPath;
    }

    public RecordingDetailViewModel GetRecordingDetail(string fileName)
    {
        var filePath = GetSafeFilePath(fileName);
        var fileInfo = new FileInfo(filePath);

        return new RecordingDetailViewModel
        {
            FileName = fileInfo.Name,
            CreatedAt = fileInfo.CreationTime,
            DurationTime = new TimeOnly(0, 10),
            SizeBytes = fileInfo.Length
        };
    }

    public (bool, string) DeleteRecord(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(settings.RecordingsFolder, safeFileName);

        if (!File.Exists(fullPath))
        {
            return (false, "Soubor nebyl nalezen.");
        }

        try
        {
            File.Delete(fullPath);
            return (true, "Soubor úspěšně smazán.");
        }
        catch (Exception ex)
        {
            return (false, $"Soubor se nepodařilo odstranit. Error: {ex.Message}.");
        }
    }
}