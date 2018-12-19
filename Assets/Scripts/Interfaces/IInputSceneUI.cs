using System;
using System.Collections;

public interface IInputSceneUI
{
  void StartLoadingPlayScene();
  void ShowRecordingProcess(int currentTurn, int gameLength);
  void ShowNotiPanel(string text, float delay, float duration);
  void ShowRecordPanelWhenError(string errorMessage);
  void ShowMapInfo(MapDisplayData mapInfo);
  Action<int> SetIsBot{set;}
  Action<bool> ChangeMap{set;}
  Action StartGame{set;}
}