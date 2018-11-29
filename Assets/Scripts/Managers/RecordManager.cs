using System.Collections.Generic;
using UnityEngine;

public class RecordManager : IReplayRecorder
{
  public void LogEndGame(ServerGameState serverGameState)
  {
    Debug.Log("End game!!!");
  }

  public void LogGameState(ServerGameState serverGameState)
  {
    Debug.Log("Recorded!");
  }

  public void LogTurn(List<TurnAction> actions)
  {
    
  }
}