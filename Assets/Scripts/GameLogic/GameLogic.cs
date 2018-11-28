using System.Collections.Generic;
using System.Linq;

public class GameLogic
{
  ServerGameState serverGameState;
  public GameLogic()
  {
    serverGameState = new ServerGameState();
    InitializeCharacters(serverGameState);
    InitializeMap(serverGameState);
  }

  void DoStart()
  {
    // TODO Start shits here

  }

  void DoTurn()
  {
    serverGameState.turn++;

    // TODO Turn shits here

    if (serverGameState.turn < GameConfigs.GAME_LENGTH) DoTurn();
    else DoEnd();
  }

  void DoEnd() { }

  ServerGameState InitializeCharacters(ServerGameState gameState)
  {
    gameState.teamRed.Add(new Character(
      (int)GameConfigs.RED_BOX_POS.X,
      (int)GameConfigs.RED_BOX_POS.Y,
      Team.Red,
      CharacterClass.Planter
    ));
    gameState.teamRed.Add(new Character(
      (int)GameConfigs.RED_BOX_POS.X,
      (int)GameConfigs.RED_BOX_POS.Y,
      Team.Red,
      CharacterClass.Harvester
    ));
    gameState.teamRed.Add(new Character(
      (int)GameConfigs.RED_ROCK_POS.X,
      (int)GameConfigs.RED_ROCK_POS.Y,
      Team.Red,
      CharacterClass.Worm
    ));

    gameState.teamBlue.Add(new Character(
      (int)GameConfigs.BLUE_BOX_POS.X,
      (int)GameConfigs.BLUE_BOX_POS.Y,
      Team.Blue,
      CharacterClass.Planter
    ));
    gameState.teamBlue.Add(new Character(
      (int)GameConfigs.BLUE_BOX_POS.X,
      (int)GameConfigs.BLUE_BOX_POS.Y,
      Team.Blue,
      CharacterClass.Harvester
    ));
    gameState.teamBlue.Add(new Character(
      (int)GameConfigs.BLUE_ROCK_POS.X,
      (int)GameConfigs.BLUE_ROCK_POS.Y,
      Team.Blue,
      CharacterClass.Worm
    ));

    return gameState;
  }

  ServerGameState InitializeMap(ServerGameState gameState)
  {
    for (int x = 0; x < GameConfigs.MAP_WIDTH; x++)
    {
      gameState.map.Add(new List<Tile>());
      for (int y = 0; y < GameConfigs.MAP_WIDTH; y++)
      {
        var tile = new Tile(x, y);
        if (tile == GameConfigs.RED_BOX_POS)
        {
          tile.alwaysVisible = true;
          tile.type = TileType.RED_BOX;
        }
        else if (tile == GameConfigs.BLUE_BOX_POS)
        {
          tile.type = TileType.BLUE_BOX;
          tile.alwaysVisible = true;
        }
        else if (tile == GameConfigs.BLUE_ROCK_POS)
        {
          tile.alwaysVisible = true;
          tile.type = TileType.BLUE_ROCK;
        }
        else if (tile == GameConfigs.RED_ROCK_POS)
        {
          tile.alwaysVisible = true;
          tile.type = TileType.RED_ROCK;
        }
        else if (GameConfigs.WILDBERRY_POS.Any(pos => tile == pos))
        {
          tile.alwaysVisible = true;
          tile.type = TileType.WILDBERRY;
          tile.growState = GameConfigs.WILDBERRY_FRUIT_TIME;
        }
        else
          tile.type = TileType.EMPTY;

        gameState.map[x].Add(tile);
      }
    }

    return gameState;
  }
}