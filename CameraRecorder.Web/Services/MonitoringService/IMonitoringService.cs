using CameraRecorder.Web.ViewModels.Monitoring;

namespace CameraRecorder.Web.Services.MonitoringService;

public interface IMonitoringService
{
    MonitoringTimelineViewModel GetTimeline(DateOnly selectedDate);
}