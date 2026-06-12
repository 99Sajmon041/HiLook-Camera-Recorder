using CameraRecorder.Web.Settings;
using Microsoft.Extensions.Options;

namespace CameraRecorder.Web.Services.RecordingFile;

public sealed class RecordingFileService(
    IOptions<RecordingStorageSettings> options) : IRecordingFileService
{
    private readonly RecordingStorageSettings settings = options.Value;

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
}