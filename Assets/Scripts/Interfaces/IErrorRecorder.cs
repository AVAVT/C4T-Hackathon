public interface IErrorRecorder
{
  bool HaveError{get;}
  void RecordErrorMessage(string message, bool isRecordingError);
}