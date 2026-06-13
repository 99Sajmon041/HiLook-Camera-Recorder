namespace CameraRecorder.Web.ViewModels.Monitoring;

public sealed class TimelineSlotViewModel
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? FileName { get; set; }
    public bool HasRecording => !string.IsNullOrWhiteSpace(FileName);
    public bool HasMotion { get; set; }
}
