using System;
using System.Collections.Generic;

public interface IPlaySceneUI
{
  Action<bool> PlayLog { set; }
  Action StopLog { set; }
  Action NextTurn { set; }
  Action PrevTurn { set; }
  Action<int> ChangeTurn { set; }
  Action<float> ChangeGameSpeed { set; }
  void ToggleResult(bool isShowingResult);
  void DisplayTurnInfo(int currentTurn, int blueScore, int redScore);
  void DisplayGameInfo(List<string> listNames, List<RecordModel> logs);
  void DisplayCharacterStatus(int index, string status);
}