using System;

public class ErrorRecorder : IErrorRecorder
{
  private bool haveError = false;
  public string ErrorMessage{get; private set;} 
  public bool HaveError
  {
    get
    {
      return haveError;
    }
  }

  public void RecordErrorMessage(string message, bool isRecordingError)
  {
    if (isRecordingError)
    {
      this.haveError = true;
      ErrorMessage += message + Environment.NewLine + Environment.NewLine;
    }
    if (!isRecordingError && !this.haveError) ErrorMessage += message + Environment.NewLine + Environment.NewLine;
  }
}