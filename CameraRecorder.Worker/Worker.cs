using CameraRecorder.Worker.Services;
using CameraRecorder.Worker.Settings;
using Microsoft.Extensions.Options;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly CameraSettings cameraSettings;
    private readonly FfmpegRecorderService ffmpegRecorderService;
    private readonly CameraEventService cameraEventService;
    private readonly MotionEventService motionEventService;

    public Worker(
        ILogger<Worker> logger,
        IOptions<CameraSettings> cameraSettings,
        FfmpegRecorderService ffmpegRecorderService,
        CameraEventService cameraEventService,
        MotionEventService motionEventService)
    {
        this.logger = logger;
        this.cameraSettings = cameraSettings.Value;
        this.ffmpegRecorderService = ffmpegRecorderService;
        this.cameraEventService = cameraEventService;
        this.motionEventService = motionEventService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Camera recorder started.");
        logger.LogInformation("Camera name: {name}", cameraSettings.Name);
        logger.LogInformation("Output folder: {folder}", cameraSettings.OutputFolder);

        var recordingTask = RunRecordingAsync(stoppingToken);
        var motionTask = RunMotionDetectionAsync(stoppingToken);

        await Task.WhenAll(recordingTask, motionTask);
    }

    private async Task RunRecordingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ffmpegRecorderService.RecordSegmentAsync(stoppingToken);
                ffmpegRecorderService.DeleteOldRecordings();
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Recording stopped.");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during recording.");
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }
    }

    private async Task RunMotionDetectionAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await cameraEventService.DetectMotionAsync(stoppingToken);

                if (result.IsMotionDetected)
                {
                    await motionEventService.SaveMotionEventAsync(
                        result.DifferencePercent,
                        stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Motion detection stopped.");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during motion detection.");
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }
    }
}