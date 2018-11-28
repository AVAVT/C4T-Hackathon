using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class GameLogic
{
  ServerGameState serverGameState;
  // ICharacterController redPlanterAI;
  // ICharacterController redHarvesterAI;
  // ICharacterController redWormAI;
  // ICharacterController bluePlanterAI;
  // ICharacterController blueHarvesterAI;
  // ICharacterController blueWormAI;

  List<ICharacterController> controllers = new List<ICharacterController>();
  public GameLogic(
    ICharacterController redPlanterAI,
    ICharacterController redHarvesterAI,
    ICharacterController redWormAI,
    ICharacterController bluePlanterAI,
    ICharacterController blueHarvesterAI,
    ICharacterController blueWormAI
  )
  {
    // this.redPlanterAI = redPlanterAI;
    // this.redHarvesterAI = redHarvesterAI;
    // this.redWormAI = redWormAI;
    // this.bluePlanterAI = bluePlanterAI;
    // this.blueHarvesterAI = blueHarvesterAI;
    // this.blueWormAI = blueWormAI;

    controllers.Add(redPlanterAI);
    controllers.Add(redHarvesterAI);
    controllers.Add(redWormAI);
    controllers.Add(bluePlanterAI);
    controllers.Add(blueHarvesterAI);
    controllers.Add(blueWormAI);

    serverGameState = new ServerGameState();

    InitializeCharacters(serverGameState);
    InitializeMap(serverGameState);
    DoStart();
  }

  void DoStart()
  {
    foreach (var controller in controllers) controller.DoStart(serverGameState.GameStateForTeam(controller.Character.team));
    DoTurn();
  }

  void DoTurn()
  {
    serverGameState.turn++;

    // TODO Turn shits here

    if (serverGameState.turn < GameConfigs.GAME_LENGTH) DoTurn();
    else DoEnd();
  }

  void DoEnd()
  {
    // TODO
  }

  ServerGameState InitializeCharacters(ServerGameState gameState)
  {
    InitializeTeam(Team.Red, gameState.teamRed, GameConfigs.RED_STARTING_POSES, 0);
    InitializeTeam(Team.Blue, gameState.teamBlue, GameConfigs.BLUE_STARTING_POSES, 3);

    return gameState;
  }

  void InitializeTeam(Team team, List<Character> teamList, List<Vector2> startingPoses, int controllerIndexInc)
  {
    for (int i = 0; i < 3; i++)
    {
      var pos = startingPoses[i];
      var character = new Character(
          (int)pos.X,
          (int)pos.Y,
          team,
          (CharacterRole)i
        );
      teamList.Add(character);
      controllers[i + controllerIndexInc].Character = character;
    }
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