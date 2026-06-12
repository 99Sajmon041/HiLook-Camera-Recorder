using CameraRecorder.Worker.Models;
using CameraRecorder.Worker.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;


namespace CameraRecorder.Worker.Services;

public sealed class MotionEventService(IOptions<CameraSettings> cameraSettings, ILogger<MotionEventService> logger)
{
    private readonly CameraSettings cameraSettings = cameraSettings.Value;
    private readonly ILogger<MotionEventService> logger = logger;

    public async Task SaveMotionEventAsync(double differencePercent, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cameraSettings.MotionEventsFilePath))
        {
            throw new InvalidOperationException("Motion events file path is not configured.");
        }

        var motionEvent = new MotionEvent
        {
            DetectedAt = DateTime.Now,
            DifferencePercent = differencePercent
        };

        var folder = Path.GetDirectoryName(cameraSettings.MotionEventsFilePath);

        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var jsonLine = JsonSerializer.Serialize(motionEvent);

        await File.AppendAllTextAsync(cameraSettings.MotionEventsFilePath, jsonLine + Environment.NewLine, ct);

        logger.LogInformation("Motion event saved. Time: {time}, Difference: {difference:F2}%", motionEvent.DetectedAt, differencePercent);
    }
}
