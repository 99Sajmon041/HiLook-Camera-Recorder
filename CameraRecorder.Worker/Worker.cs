using CameraRecorder.Worker.Services;
using CameraRecorder.Worker.Settings;
using Microsoft.Extensions.Options;

namespace CameraRecorder.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly CameraSettings cameraSettings;
    private readonly FfmpegRecorderService ffmpegRecorderService;

    public Worker(ILogger<Worker> logger, IOptions<CameraSettings> cameraSettings, FfmpegRecorderService ffmpegRecorderService)
    {
        this.logger = logger;
        this.cameraSettings = cameraSettings.Value;
        this.ffmpegRecorderService = ffmpegRecorderService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Camera recorder started.");
        logger.LogInformation("Camera name: {name}", cameraSettings.Name);
        logger.LogInformation("Output folder: {folder}", cameraSettings.OutputFolder);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ffmpegRecorderService.RecordSegmentAsync(stoppingToken);

            ffmpegRecorderService.DeleteOldRecordings();

            logger.LogInformation("Segment finished.");
        }
    }
}