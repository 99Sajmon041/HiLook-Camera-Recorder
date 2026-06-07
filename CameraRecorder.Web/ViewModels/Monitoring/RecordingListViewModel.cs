using CameraRecorder.Web.Pagination;

namespace CameraRecorder.Web.ViewModels.Monitoring;

public sealed class RecordingListViewModel
{
    public DateOnly SelectedDate { get; set; }
    public PagedResult<RecordingListItemViewModel> Records { get; set; } = default!;
}