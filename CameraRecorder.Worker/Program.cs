using CameraRecorder.Worker;
using CameraRecorder.Worker.Services;
using CameraRecorder.Worker.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.Configure<CameraSettings>(
    builder.Configuration.GetSection("CameraSettings"));

builder.Services.Configure<IsapiSettings>(
    builder.Configuration.GetSection("IsapiSettings"));

builder.Services.AddHttpClient<CameraEventService>()
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<IsapiSettings>>().Value;

        return new HttpClientHandler
        {
            Credentials = new NetworkCredential(settings.Username, settings.Password),
            PreAuthenticate = true
        };
    });

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Camera Recorder Worker";
});

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<FfmpegRecorderService>();

var host = builder.Build();
host.Run();