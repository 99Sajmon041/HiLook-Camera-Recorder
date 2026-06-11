namespace CameraRecorder.Web.ViewModels.Monitoring;

public sealed class RecordingSegmentViewModel
{
    public string FileName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }     
    public DateTime EndTime { get; set; }
    public long SizeBytes { get; set; }
}