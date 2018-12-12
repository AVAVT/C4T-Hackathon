using System;
using System.Collections;

public interface IInputSceneUI
{
  void ShowOutputText(string text);
  IEnumerator StartLoadingPlayScene();
  void ShowFileStatus(int index);
  void ShowRecordingProcess(float process);
  void ShowNotiPanel(string text);
  void ShowRecordPanelWhenError();
  Action StartGame{set;}
  Action<int> LoadAIFolder{set;}
}