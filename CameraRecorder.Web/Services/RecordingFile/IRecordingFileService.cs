namespace CameraRecorder.Web.Services.RecordingFile;

public interface IRecordingFileService
{
    string GetSafeFilePath(string fileName);
}