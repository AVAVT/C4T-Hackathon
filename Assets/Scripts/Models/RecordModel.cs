using System.Collections.Generic;

[System.Serializable]
public struct RecordModel
{
  public ServerGameState serverGameState;
  public List<TurnAction> actions;
}