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

        var finalFile = Path.Combine(cameraSettings.OutputFolder, fileName);
        var temporaryFile = Path.ChangeExtension(finalFile, ".recording");

        var arguments = $"-rtsp_transport tcp -i \"{cameraSettings.RtspUrl}\" -t {cameraSettings.SegmentMinutes * 60} -c copy -f mp4 \"{temporaryFile}\"";
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

            throw;
        }

        var error = await errorTask;

        if (process.ExitCode == 0)
        {
            File.Move(temporaryFile, finalFile, overwrite: true);

            logger.LogInformation("FFmpeg finished successfully.");
            logger.LogInformation("FFmpeg log: {error}", error);
        }
        else
        {
            logger.LogError("FFmpeg failed with exit code {exitCode}. Error: {error}", process.ExitCode, error);
        }
    }

    public async Task RecordBufferSegmentAsync(CancellationToken ct)
    {
        ValidateSettings();

        Directory.CreateDirectory(cameraSettings.TempBufferFolder);

        var fileName = $"buffer_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";

        var finalFile = Path.Combine(cameraSettings.TempBufferFolder, fileName);
        var temporaryFile = Path.ChangeExtension(finalFile, ".recording");

        var arguments = $"-rtsp_transport tcp -i \"{cameraSettings.RtspUrl}\" -t {cameraSettings.BufferSegmentSeconds} -c copy -f mp4 \"{temporaryFile}\"";

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
            throw new InvalidOperationException("FFmpeg buffer process could not be started.");
        }

        logger.LogInformation("FFmpeg buffer recording started.");

        var errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(ct);

        var error = await errorTask;

        if (process.ExitCode == 0)
        {
            File.Move(temporaryFile, finalFile, overwrite: true);

            logger.LogInformation("FFmpeg buffer segment finished: {file}", fileName);
        }
        else
        {
            logger.LogError("FFmpeg buffer failed with exit code {exitCode}. Error: {error}", process.ExitCode, error);
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

    public void SavePreMotionBuffer()
    {
        ValidateSettings();

        if (!Directory.Exists(cameraSettings.TempBufferFolder))
        {
            return;
        }

        Directory.CreateDirectory(cameraSettings.OutputFolder);

        var segmentsCount = (int)Math.Ceiling(
            (double)cameraSettings.PreMotionSeconds / cameraSettings.BufferSegmentSeconds);

        var files = Directory
            .GetFiles(cameraSettings.TempBufferFolder, "*.mp4")
            .Select(file => new FileInfo(file))
            .OrderByDescending(file => file.CreationTime)
            .Take(segmentsCount)
            .OrderBy(file => file.CreationTime)
            .ToList();

        foreach (var file in files)
        {
            var newFileName = file.Name.Replace("buffer_", "camera_");
            var destinationPath = Path.Combine(cameraSettings.OutputFolder, newFileName);

            if (!File.Exists(destinationPath))
            {
                File.Copy(file.FullName, destinationPath);
                logger.LogInformation("Saved pre-motion buffer segment: {file}", newFileName);
            }
        }
    }

    public void DeleteOldBufferSegments()
    {
        if (!Directory.Exists(cameraSettings.TempBufferFolder))
        {
            return;
        }

        var keepSeconds = cameraSettings.PreMotionSeconds + cameraSettings.BufferSegmentSeconds;

        var deleteOlderThan = DateTime.Now.AddSeconds(-keepSeconds);

        var files = Directory
            .GetFiles(cameraSettings.TempBufferFolder, "*.mp4")
            .Select(file => new FileInfo(file));

        foreach (var file in files)
        {
            if (file.CreationTime < deleteOlderThan)
            {
                file.Delete();
                logger.LogInformation("Deleted old buffer segment: {file}", file.Name);
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

        if (string.IsNullOrWhiteSpace(cameraSettings.TempBufferFolder))
        {
            throw new InvalidOperationException("Temp buffer folder is not configured.");
        }

        if (cameraSettings.BufferSegmentSeconds <= 0)
        {
            throw new InvalidOperationException("Buffer segment seconds must be greater than zero.");
        }

        if (cameraSettings.PreMotionSeconds <= 0)
        {
            throw new InvalidOperationException("Pre-motion seconds must be greater than zero.");
        }
    }
}
