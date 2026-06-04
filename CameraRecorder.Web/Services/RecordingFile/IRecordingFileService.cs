using CameraRecorder.Web.ViewModels.Monitoring;

namespace CameraRecorder.Web.Services.RecordingFile;

public interface IRecordingFileService
{
    RecordingListViewModel GetRecordingsByDate(DateOnly selectedDate);
    RecordingDetailViewModel GetRecordingDetail(string fileName);
    string GetSafeFilePath(string fileName);
    (bool, string) DeleteRecord(string fileName);
}