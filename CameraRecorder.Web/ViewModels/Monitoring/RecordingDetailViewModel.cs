namespace CameraRecorder.Web.ViewModels.Monitoring;

public sealed class RecordingDetailViewModel
{
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeBytes { get; set; }
}