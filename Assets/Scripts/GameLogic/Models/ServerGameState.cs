using System.Collections.Generic;

[System.Serializable]
public class ServerGameState
{
  public int turn = 0;
  public int redScore = 0;
  public int blueScore = 0;
  public TeamRoleMap<Character> characters = new TeamRoleMap<Character>();
  public List<List<Tile>> map = new List<List<Tile>>();

  public GameState GameStateForTeam(Team team, GameConfig gameRule)
  {
    var result = new GameState(turn, team);

    result.allies.AddRange(characters.GetItemsBy(team).Values);

    foreach (Character enemy in characters)
    {
      if (team == enemy.team) continue;

      foreach (var ally in result.allies)
      {
        if (enemy.DistanceTo(ally) <= gameRule.sightDistance)
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
            if (ally.DistanceTo(teamTile) <= gameRule.sightDistance)
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