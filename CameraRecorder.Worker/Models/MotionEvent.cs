namespace CameraRecorder.Worker.Models;

public sealed class MotionEvent
{
    public DateTime DetectedAt { get; set; }
    public double DifferencePercent { get; set; }
}
