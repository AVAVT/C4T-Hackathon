using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameLogic
{
  public ServerGameState ServerGameState { get; private set; }
  MapInfo mapInfo;
  GameRule gameRule;
  public Dictionary<Team, Dictionary<CharacterRole, ICharacterController>> characterControllers;

  public static GameLogic GameLogicForPlay(GameRule gameRule, MapInfo mapInfo)
  {
    return new GameLogic(gameRule, mapInfo);
  }

  public static GameLogic GameLogicForViewReplay(GameRule gameRule, ServerGameState replayGameState)
  {
    return new GameLogic(gameRule, replayGameState);
  }

  private GameLogic(GameRule gameRule, MapInfo mapInfo)
  {
    ServerGameState = new ServerGameState();
    this.mapInfo = mapInfo;
    this.gameRule = gameRule;
  }

  private GameLogic(GameRule gameRule, ServerGameState replayGameState)
  {
    ServerGameState = replayGameState;
    this.gameRule = gameRule;
  }

  public void InitializeGame(
    Dictionary<Team, Dictionary<CharacterRole, ICharacterController>> characterControllers
  )
  {
    InitializeCharacters(ServerGameState, mapInfo);
    InitializeMap(ServerGameState, mapInfo);
    this.characterControllers = characterControllers;

    AssignCharacterToControllers(ServerGameState, characterControllers);
  }

  public async Task PlayGame(CancellationToken cancellationToken, IReplayRecorder recorder = null)
  {
    await DoStart(recorder);

    ServerGameState.turn++;

    while (ServerGameState.turn < gameRule.gameLength && !cancellationToken.IsCancellationRequested)
    {
      await PlayNextTurn(recorder);
    }

    DoEnd(recorder);
  }

  async Task DoStart(IReplayRecorder recorder)
  {
    foreach (var kvp in characterControllers)
    {
      GameState teamGameState = ServerGameState.GameStateForTeam(kvp.Key, gameRule);

      foreach (var controller in kvp.Value.Values)
      {
        await controller.DoStart(teamGameState, gameRule);
      }
    }
    recorder?.LogGameStart(gameRule, ServerGameState);
  }

  async Task PlayNextTurn(IReplayRecorder recorder)
  {
    List<TurnAction> actions = new List<TurnAction>();

    foreach (var team in characterControllers)
    {
      var teamGameState = ServerGameState.GameStateForTeam(team.Key, gameRule);

      foreach (var controller in team.Value.Values)
      {
        var result = await controller.DoTurn(teamGameState, gameRule);
        actions.Add(new TurnAction(
          controller.Character.team,
          controller.Character.characterRole,
          result,
          controller.IsTimedOut,
          controller.IsCrashed
        ));
      }
    }

    ExecuteTurn(actions);
    recorder?.LogTurn(ServerGameState, actions);
    ServerGameState.turn++;
  }

  /// <summary>Progress the game state with given actions</summary>
  public void ExecuteTurn(List<TurnAction> actions)
  {
    DoMove(actions);
    DoCatchWorm(ServerGameState);
    DoScareHarvester(ServerGameState);
    DoPlantTree(ServerGameState);
    DoHarvest(ServerGameState);
    DoGetPoint(ServerGameState);
    DoDestroyPlant(ServerGameState);
    DoGrowPlant(ServerGameState);
  }

  void DoEnd(IReplayRecorder recorder)
  {
    recorder?.LogEndGame(ServerGameState);
  }
  void DoGrowPlant(ServerGameState serverGameState)
  {
    for (int row = 0; row < serverGameState.map.Count; row++)
    {
      for (int col = 0; col < serverGameState.map[row].Count; col++)
      {
        var tile = serverGameState.map[row][col];

        if (IsUnripePlant(tile)) tile.growState++;

        serverGameState.map[row][col] = tile;
      }
    }
  }

  bool IsUnripePlant(Tile tile)
  {
    return (tile.type == TileType.WILDBERRY && tile.growState < gameRule.wildberryFruitTime)
    || ((tile.type == TileType.PUMPKIN || tile.type == TileType.TOMATO) && tile.growState < gameRule.plantFruitTime);
  }

  void DoGetPoint(ServerGameState serverGameState)
  {
    foreach (var team in serverGameState.characters)
    {
      var character = team.Value[CharacterRole.Harvester];
      var currentTile = serverGameState.map[character.x][character.y];
      var allyScoreTileType = team.Key == Team.Red ? TileType.RED_BOX : TileType.BLUE_BOX;

      if (currentTile.type == allyScoreTileType)
      {
        if (team.Key == Team.Red) serverGameState.redScore += character.fruitCarrying;
        else serverGameState.blueScore += character.fruitCarrying;

        character.numFruitDelivered += character.fruitCarrying;
        character.fruitCarrying = 0;
        team.Value[CharacterRole.Harvester] = character;
      }
    }
  }
  void DoHarvest(ServerGameState serverGameState)
  {
    foreach (var team in serverGameState.characters)
    {
      var character = team.Value[CharacterRole.Harvester];
      var currentTile = serverGameState.map[character.x][character.y];
      var teamFruitType = team.Key == Team.Red ? TileType.TOMATO : TileType.PUMPKIN;

      if (IsTileRipeHarvestableFruit(currentTile, teamFruitType))
      {
        currentTile.growState = 1;
        serverGameState.map[character.x][character.y] = currentTile;

        character.fruitCarrying += gameRule.harvestValue[currentTile.type];
        character.numFruitHarvested++;
        serverGameState.characters[team.Key][CharacterRole.Harvester] = character;
      }
    }
  }

  bool IsTileRipeHarvestableFruit(Tile tile, TileType teamFruitType)
  {
    return (tile.type == teamFruitType && tile.growState == gameRule.plantFruitTime)
      || (tile.type == TileType.WILDBERRY && tile.growState == gameRule.wildberryFruitTime);
  }

  void DoPlantTree(ServerGameState serverGameState)
  {
    foreach (var team in serverGameState.characters)
    {
      var character = team.Value[CharacterRole.Planter];
      var currentTile = serverGameState.map[character.x][character.y];
      if (currentTile.type == TileType.EMPTY)
      {
        currentTile.type = team.Key == Team.Red ? TileType.TOMATO : TileType.PUMPKIN;
        serverGameState.map[character.x][character.y] = currentTile;

        var planter = serverGameState.characters[team.Key][CharacterRole.Planter];
        planter.numTreePlanted++;
        team.Value[CharacterRole.Planter] = planter;
      }
    }
  }

  void DoDestroyPlant(ServerGameState serverGameState)
  {
    foreach (var team in serverGameState.characters)
    {
      var character = team.Value[CharacterRole.Worm];
      var currentTile = serverGameState.map[character.x][character.y];
      var destroyablePlantType = team.Key == Team.Red ? TileType.PUMPKIN : TileType.TOMATO;
      if (currentTile.type == destroyablePlantType)
      {
        currentTile.type = TileType.EMPTY;
        currentTile.growState = 0;
        serverGameState.map[character.x][character.y] = currentTile;

        character.numTreeDestroyed++;
        team.Value[CharacterRole.Worm] = character;
      }
    }
  }
  void DoScareHarvester(ServerGameState serverGameState)
  {
    foreach (var wormTeam in serverGameState.characters)
    {
      foreach (var harvesterTeam in serverGameState.characters)
      {
        if (wormTeam.Key == harvesterTeam.Key) continue; // Don't scare ally

        var worm = wormTeam.Value[CharacterRole.Worm];
        var harvester = harvesterTeam.Value[CharacterRole.Harvester];
        if (worm.DistanceTo(harvester) == 0)
        {
          harvester.isScared = true;
          harvesterTeam.Value[CharacterRole.Harvester] = harvester;

          worm.numHarvesterScared++;
          wormTeam.Value[CharacterRole.Worm] = worm;
        }
      }
    }
  }

  void DoCatchWorm(ServerGameState serverGameState)
  {
    foreach (var planterTeam in serverGameState.characters)
    {
      foreach (var wormTeam in serverGameState.characters)
      {
        if (planterTeam.Key == wormTeam.Key) continue; // Don't catch ally

        var worm = wormTeam.Value[CharacterRole.Worm];
        var planter = planterTeam.Value[CharacterRole.Planter];

        if (worm.DistanceTo(planter) == 0)
        {
          var wormNewPosition = mapInfo.startingPositions[worm.team][CharacterRole.Worm];
          worm.x = (int)wormNewPosition.X;
          worm.y = (int)wormNewPosition.Y;
          wormTeam.Value[CharacterRole.Worm] = worm;

          planter.numWormCaught++;
          planterTeam.Value[CharacterRole.Planter] = planter;
        }
      }
    }
  }

  void DoMove(List<TurnAction> actions)
  {
    Dictionary<Team, Dictionary<CharacterRole, Character>> targetPoses = new Dictionary<Team, Dictionary<CharacterRole, Character>>();
    targetPoses = ObjectExtensions.Clone(ServerGameState.characters);
    foreach (var action in actions)
    {
      var character = targetPoses[action.team][action.role];
      var newPos = new Vector2(character.x, character.y);
      if (!character.isScared)
        newPos += action.direction.ToDirectionVector();
      else
        character.isScared = false;

      character.x = Math.Max(Math.Min((int)newPos.X, mapInfo.tiles.Count - 1), 0);
      character.y = Math.Max(Math.Min((int)newPos.Y, mapInfo.tiles[character.x].Count - 1), 0);

      if (!IsInImpassableTile(ServerGameState, character))
      {
        targetPoses[action.team][action.role] = character;
      }
    }

    CancelMovementForCounterRolesSwapingPlaces(targetPoses, actions);

    ServerGameState.characters = targetPoses;
  }

  // TODO refactor to prevent changing actions
  void CancelMovementForCounterRolesSwapingPlaces(Dictionary<Team, Dictionary<CharacterRole, Character>> targetPoses, List<TurnAction> actions)
  {
    if (AreSwapingPlaces(targetPoses, Team.Red, CharacterRole.Worm, Team.Blue, CharacterRole.Planter))
    {
      targetPoses[Team.Red][CharacterRole.Worm] = ServerGameState.characters[Team.Red][CharacterRole.Worm];
      SetActionToStay(actions, Team.Red, CharacterRole.Worm);
    }
    else if (AreSwapingPlaces(targetPoses, Team.Red, CharacterRole.Worm, Team.Blue, CharacterRole.Harvester))
    {
      targetPoses[Team.Blue][CharacterRole.Harvester] = ServerGameState.characters[Team.Blue][CharacterRole.Harvester];
      SetActionToStay(actions, Team.Blue, CharacterRole.Harvester);
    }

    if (AreSwapingPlaces(targetPoses, Team.Blue, CharacterRole.Worm, Team.Red, CharacterRole.Planter))
    {
      targetPoses[Team.Blue][CharacterRole.Worm] = ServerGameState.characters[Team.Blue][CharacterRole.Worm];
      SetActionToStay(actions, Team.Blue, CharacterRole.Worm);
    }
    else if (AreSwapingPlaces(targetPoses, Team.Blue, CharacterRole.Worm, Team.Red, CharacterRole.Harvester))
    {
      targetPoses[Team.Red][CharacterRole.Harvester] = ServerGameState.characters[Team.Red][CharacterRole.Harvester];
      SetActionToStay(actions, Team.Red, CharacterRole.Harvester);
    }
  }

  bool AreSwapingPlaces(Dictionary<Team, Dictionary<CharacterRole, Character>> targetPoses, Team char1Team, CharacterRole char1Role, Team char2Team, CharacterRole char2Role)
  {
    return targetPoses[char1Team][char1Role].DistanceTo(ServerGameState.characters[char2Team][char2Role]) == 0
          && targetPoses[char2Team][char2Role].DistanceTo(ServerGameState.characters[char1Team][char1Role]) == 0;
  }
  void SetActionToStay(List<TurnAction> listActions, Team team, CharacterRole characterRole)
  {
    for (int i = 0; i < listActions.Count; i++)
    {
      if (listActions[i].role == characterRole && listActions[i].team == team)
      {
        var temp = listActions[i];
        temp.direction = Directions.STAY;
        listActions[i] = temp;
        break;
      }
    }
  }

  bool IsInImpassableTile(ServerGameState serverGameState, Character character)
  {
    return character.DistanceTo(serverGameState.map[character.x][character.y]) == 0 && serverGameState.map[character.x][character.y].type == TileType.IMPASSABLE;
  }

  ServerGameState InitializeCharacters(ServerGameState gameState, MapInfo mapInfo)
  {
    InitializeTeam(Team.Red, gameState, mapInfo.startingPositions);
    InitializeTeam(Team.Blue, gameState, mapInfo.startingPositions);

    return gameState;
  }

  void InitializeTeam(Team team, ServerGameState gameState, Dictionary<Team, Dictionary<CharacterRole, Vector2>> startingPoses)
  {
    gameState.characters.Add(team, new Dictionary<CharacterRole, Character>());
    for (int i = 0; i < 3; i++)
    {
      CharacterRole role = (CharacterRole)i;

      var pos = startingPoses[team][role];
      var character = new Character(
          (int)pos.X,
          (int)pos.Y,
          team,
          role
        );
      gameState.characters[team].Add(role, character);
    }
  }

  void AssignCharacterToControllers(ServerGameState gameState, Dictionary<Team, Dictionary<CharacterRole, ICharacterController>> controllers)
  {
    foreach (var team in gameState.characters.Keys)
    {
      foreach (var kvp in gameState.characters[team])
      {
        controllers[team][kvp.Key].Character = kvp.Value;
      }
    }
  }

  ServerGameState InitializeMap(ServerGameState gameState, MapInfo mapInfo)
  {
    for (int x = 0; x < mapInfo.tiles.Count; x++)
    {
      gameState.map.Add(new List<Tile>());
      for (int y = 0; y < mapInfo.tiles[x].Count; y++)
      {
        var tile = new Tile(x, y);
        tile.type = mapInfo.tiles[x][y];

        if (IsTileTypeAlwaysVisible(tile.type)) tile.alwaysVisible = true;

        if (tile.type == TileType.WILDBERRY)
        {
          tile.growState = gameRule.wildberryFruitTime;
        }

        gameState.map[x].Add(tile);
      }
    }
    return gameState;
  }

  bool IsTileTypeAlwaysVisible(TileType type)
  {
    return type == TileType.RED_BOX || type == TileType.BLUE_BOX || type == TileType.RED_ROCK || type == TileType.BLUE_ROCK;
  }
}