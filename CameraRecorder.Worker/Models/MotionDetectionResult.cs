namespace CameraRecorder.Worker.Models;

public sealed class MotionDetectionResult
{
    public bool IsMotionDetected { get; set; }
    public double DifferencePercent { get; set; }
}