using System.Collections.Generic;

public class ServerGameState
{
  public int turn = 0;
  public int redScore = 0;
  public int blueScore = 0;
  public List<Character> teamRed = new List<Character>();
  public List<Character> teamBlue = new List<Character>();
  public List<List<Tile>> map = new List<List<Tile>>();

  public GameState GameStateForTeam(Team team)
  {
    var result = new GameState(turn, team);
    var candiateEnemies = new List<Character>(team == Team.Red ? teamBlue : teamRed);
    result.allies = new List<Character>(team == Team.Red ? teamRed : teamBlue);

    foreach (var character in result.allies)
    {
      foreach (var candidate in candiateEnemies)
      {

      }
    }

    return result;
  }
}