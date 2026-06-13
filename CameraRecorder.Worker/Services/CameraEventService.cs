using CameraRecorder.Worker.Models;
using CameraRecorder.Worker.Settings;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace CameraRecorder.Worker.Services;

public sealed class CameraEventService(ILogger<CameraEventService> logger, IOptions<IsapiSettings> isapiSettings, HttpClient httpClient)
{
    private readonly ILogger<CameraEventService> logger = logger;
    private readonly IsapiSettings isapiSettings = isapiSettings.Value;
    private readonly HttpClient httpClient = httpClient;

    public async Task<MotionDetectionResult> DetectMotionAsync(CancellationToken ct)
    {
        var url = $"{isapiSettings.BaseUrl}/ISAPI/Streaming/channels/101/picture";

        var image1Bytes = await httpClient.GetByteArrayAsync(url, ct);

        await Task.Delay(TimeSpan.FromMilliseconds(500), ct);

        var image2Bytes = await httpClient.GetByteArrayAsync(url, ct);

        using var image1 = Image.Load<Rgba32>(image1Bytes);
        using var image2 = Image.Load<Rgba32>(image2Bytes);

        var width = Math.Min(image1.Width, image2.Width);
        var height = Math.Min(image1.Height, image2.Height);

        var changedPixels = 0;
        var checkedPixels = 0;

        for (var y = 0; y < height; y += 10)
        {
            for (var x = 0; x < width; x += 10)
            {
                var pixel1 = image1[x, y];
                var pixel2 = image2[x, y];

                var diff =
                    Math.Abs(pixel1.R - pixel2.R) +
                    Math.Abs(pixel1.G - pixel2.G) +
                    Math.Abs(pixel1.B - pixel2.B);

                if (diff > 50)
                {
                    changedPixels++;
                }

                checkedPixels++;
            }
        }

        var differencePercent = (double)changedPixels / checkedPixels * 100;

        logger.LogInformation("Motion difference: {difference:F2}%", differencePercent);

        return new MotionDetectionResult
        {
            IsMotionDetected = differencePercent > 10,
            DifferencePercent = differencePercent
        };
    }
}