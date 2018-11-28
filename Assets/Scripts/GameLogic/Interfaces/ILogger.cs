using System.Collections.Generic;

public interface ILogger
{
  void LogGameState(ServerGameState serverGameState);
  void LogTurn(List<TurnAction> actions);
}

public struct TurnAction
{
  public Team team;
  public CharacterRole role;
  public string direction;
}