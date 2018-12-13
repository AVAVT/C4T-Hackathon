using System;
using System.Collections;

public interface IInputSceneUI
{
  void SaveErrorMessage(string text, bool haveError);
  IEnumerator StartLoadingPlayScene();
  void ShowFileStatus(int index);
  void ShowRecordingProcess(int currentTurn, int gameLength);
  void ShowNotiPanel(string text, float delay, float duration);
  void ShowRecordPanelWhenError();
  Action StartGame{set;}
  Action<int> LoadAIFolder{set;}
  string ErrorMessage{get;}
  bool HaveError{get;}
}