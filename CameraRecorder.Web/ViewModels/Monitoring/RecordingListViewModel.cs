namespace CameraRecorder.Web.ViewModels.Monitoring;

public sealed class RecordingListViewModel
{
    public DateOnly SelectedDate { get; set; }
    public List<RecordingListItemViewModel> Recordings { get; set; } = [];
}