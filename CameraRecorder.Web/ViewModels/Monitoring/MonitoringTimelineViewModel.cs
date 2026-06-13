namespace CameraRecorder.Web.ViewModels.Monitoring;

public sealed class MonitoringTimelineViewModel
{
    public DateOnly SelectedDate { get; set; }
    public List<RecordingSegmentViewModel> Segments { get; set; } = [];
    public List<MotionEventViewModel> MotionEvents { get; set; } = [];
    public List<TimelineSlotViewModel> Slots { get; set; } = [];
}