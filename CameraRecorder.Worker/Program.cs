using CameraRecorder.Worker;
using CameraRecorder.Worker.Services;
using CameraRecorder.Worker.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CameraSettings>(
    builder.Configuration.GetSection("CameraSettings"));

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<FfmpegRecorderService>();

var host = builder.Build();
host.Run();