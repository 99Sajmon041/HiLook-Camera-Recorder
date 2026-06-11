namespace CameraRecorder.Web.ViewModels.Monitoring;

public sealed class MotionEventViewModel
{
    public DateTime DetectedAt { get; set; }
    public double DifferencePercent { get; set; }
}