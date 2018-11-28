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

    result.allies = new List<Character>(team == Team.Red ? teamRed : teamBlue);
    var enemies = (team == Team.Red ? teamBlue : teamRed);
    foreach (var enemy in enemies)
    {
      foreach (var ally in result.allies)
      {
        if (enemy.DistanceTo(ally) <= GameConfigs.SIGHT_DISTANCE)
        {
          result.enemies.Add(enemy);
          break;
        }
      }
    }

    foreach (var row in map)
    {
      var teamRow = new List<Tile>();
      result.map.Add(teamRow);
      foreach (var tile in row)
      {
        var teamTile = new Tile(tile.x, tile.y);

        if (tile.alwaysVisible)
        {
          teamTile.type = tile.type;
          teamTile.growState = tile.growState;
        }
        else
        {
          bool visible = false;
          foreach (var ally in result.allies)
          {
            if (ally.DistanceTo(teamTile) <= GameConfigs.SIGHT_DISTANCE)
            {
              visible = true;
              break;
            }
          }

          if (visible)
          {
            teamTile.type = tile.type;
            teamTile.growState = tile.growState;
          }
          else
          {
            teamTile.type = TileType.UNKNOWN;
            teamTile.growState = 0;
          }
        }

        teamRow.Add(teamTile);
      }
    }

    return result;
  }
}