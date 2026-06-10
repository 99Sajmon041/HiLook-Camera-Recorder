using CameraRecorder.Worker.Services;
using CameraRecorder.Worker.Settings;
using Microsoft.Extensions.Options;

namespace CameraRecorder.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly CameraSettings cameraSettings;
    private readonly FfmpegRecorderService ffmpegRecorderService;
    private readonly CameraEventService cameraEventService;

    public Worker(ILogger<Worker> logger, IOptions<CameraSettings> cameraSettings, FfmpegRecorderService ffmpegRecorderService, CameraEventService cameraEventService)
    {
        this.logger = logger;
        this.cameraSettings = cameraSettings.Value;
        this.ffmpegRecorderService = ffmpegRecorderService;
        this.cameraEventService = cameraEventService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Camera recorder started.");
        logger.LogInformation("Camera name: {name}", cameraSettings.Name);
        logger.LogInformation("Output folder: {folder}", cameraSettings.OutputFolder);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ffmpegRecorderService.RecordSegmentAsync(stoppingToken);

                ffmpegRecorderService.DeleteOldRecordings();
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Worker stopping.");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during recording.");

                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }
    }
}