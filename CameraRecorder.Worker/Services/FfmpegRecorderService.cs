using CameraRecorder.Worker.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace CameraRecorder.Worker.Services;

public sealed class FfmpegRecorderService
{
    private readonly CameraSettings cameraSettings;
    private readonly ILogger<FfmpegRecorderService> logger;

    public FfmpegRecorderService(IOptions<CameraSettings> cameraSettings, ILogger<FfmpegRecorderService> logger)
    {
        this.cameraSettings = cameraSettings.Value;
        this.logger = logger;
    }

    public async Task RecordSegmentAsync(CancellationToken cancellationToken)
    {
        ValidateSettings();

        Directory.CreateDirectory(cameraSettings.OutputFolder);

        var fileName = $"camera_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
        var outputFile = Path.Combine(cameraSettings.OutputFolder, fileName);

        var arguments = $"-rtsp_transport tcp -i \"{cameraSettings.RtspUrl}\" -t {cameraSettings.SegmentMinutes * 60} -c copy \"{outputFile}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = cameraSettings.FfmpegPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);

        if (process == null)
        {
            throw new InvalidOperationException("FFmpeg process could not be started.");
        }

        logger.LogInformation("FFmpeg recording started.");

        var errorTask = process.StandardError.ReadToEndAsync();
        var outputTask = process.StandardOutput.ReadToEndAsync();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cancellation requested. Stopping FFmpeg process.");

            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }

        var error = await errorTask;
        var output = await outputTask;

        if (process.ExitCode == 0)
        {
            logger.LogInformation("FFmpeg finished successfully.");
            logger.LogInformation("FFmpeg log: {error}", error);
        }
        else
        {
            logger.LogError("FFmpeg failed with exit code {exitCode}. Error: {error}", process.ExitCode, error);
        }
    }

    public void DeleteOldRecordings()
    {
        if (!Directory.Exists(cameraSettings.OutputFolder))
        {
            return;
        }

        var files = Directory.GetFiles(cameraSettings.OutputFolder, "*.mp4");

        var deleteOlderThan = DateTime.Now.AddDays(-cameraSettings.RetentionDays);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);

            if (fileInfo.CreationTime < deleteOlderThan)
            {
                logger.LogInformation("Deleting old recording: {file}", fileInfo.Name);

                fileInfo.Delete();   
            }
        }
    }
    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(cameraSettings.RtspUrl))
        {
            throw new InvalidOperationException("RTSP URL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(cameraSettings.FfmpegPath))
        {
            throw new InvalidOperationException("FFmpeg path is not configured.");
        }

        if (!File.Exists(cameraSettings.FfmpegPath))
        {
            throw new FileNotFoundException("FFmpeg executable was not found.", cameraSettings.FfmpegPath);
        }

        if (string.IsNullOrWhiteSpace(cameraSettings.OutputFolder))
        {
            throw new InvalidOperationException("Output folder is not configured.");
        }

        if (cameraSettings.SegmentMinutes <= 0)
        {
            throw new InvalidOperationException("Segment minutes must be greater than zero.");
        }

        if (cameraSettings.RetentionDays <= 0)
        {
            throw new InvalidOperationException("Retention days must be greater than zero.");
        }
    }
}
