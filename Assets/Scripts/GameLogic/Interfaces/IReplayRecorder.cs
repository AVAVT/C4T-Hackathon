using System.Collections.Generic;

public interface IReplayRecorder
{
  void LogGameState(ServerGameState serverGameState);
  void LogTurn(List<TurnAction> actions);
  void LogEndGame(ServerGameState serverGameState);
}

public struct TurnAction
{
  public Team team;
  public CharacterRole role;
  public string direction;
  public bool timedOut;
  public bool crashed;
  public TurnAction(Team team, CharacterRole role, string direction, bool timedOut, bool crashed)
  {
    this.team = team;
    this.role = role;
    this.direction = direction;
    this.timedOut = timedOut;
    this.crashed = crashed;
  }
}