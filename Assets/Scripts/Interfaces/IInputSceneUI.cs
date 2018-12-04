using System;
using System.Collections;

public interface IInputSceneUI
{
  void ShowOutputText(string text);
  void ShowPathByIndex(int index, string path);
  IEnumerator StartLoadingPlayScene();
  void ShowRecordingProcess(float process);
  Action StartGame{set;}
}