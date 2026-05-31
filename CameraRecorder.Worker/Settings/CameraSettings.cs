namespace CameraRecorder.Worker.Settings;

public class CameraSettings
{
    public string Name { get; set; } = string.Empty;
    public string RtspUrl { get; set; } = string.Empty;
    public string OutputFolder { get; set; } = string.Empty;
    public string FfmpegPath { get; set; } = string.Empty;
    public int SegmentMinutes { get; set; }
    public int RetentionDays { get; set; }
}