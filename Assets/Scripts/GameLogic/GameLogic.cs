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
  GameConfig gameRule;
  public TeamRoleMap<ICharacterController> characterControllers;

  public static GameLogic GameLogicForPlay(GameConfig gameRule, MapInfo mapInfo)
  {
    return new GameLogic(gameRule, mapInfo);
  }

  public static GameLogic GameLogicForViewReplay(GameConfig gameRule, ServerGameState replayGameState)
  {
    return new GameLogic(gameRule, replayGameState);
  }

  private GameLogic(GameConfig gameRule, MapInfo mapInfo)
  {
    ServerGameState = new ServerGameState();
    this.mapInfo = mapInfo;
    this.gameRule = gameRule;
  }

  private GameLogic(GameConfig gameRule, ServerGameState replayGameState)
  {
    ServerGameState = replayGameState;
    this.gameRule = gameRule;
  }

  public void InitializeGame(TeamRoleMap<ICharacterController> characterControllers)
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
    foreach (var team in characterControllers.GetTeams())
    {
      GameState teamGameState = ServerGameState.GameStateForTeam(team, gameRule);

      foreach (var controller in characterControllers.GetItemsBy(team).Values)
      {
        await controller.DoStart(teamGameState, gameRule);
      }
    }
    recorder?.LogGameStart(gameRule, ServerGameState);
  }

  async Task PlayNextTurn(IReplayRecorder recorder)
  {
    List<TurnAction> actions = new List<TurnAction>();

    foreach (var team in characterControllers.GetTeams())
    {
      var teamGameState = ServerGameState.GameStateForTeam(team, gameRule);

      foreach (var controller in characterControllers.GetItemsBy(team).Values)
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
    ResetCancelAction(ServerGameState, gameRule);
    ServerGameState.turn++;
  }

  private void ResetCancelAction(ServerGameState serverGameState, GameConfig gameRule)
  {
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        if (serverGameState.characters.GetItem(team, role).cancelAction)
        {
          var currentCharacter = serverGameState.characters.GetItem(team, role);
          currentCharacter.cancelAction = false;
          serverGameState.characters.SetItem(team, role, currentCharacter);
        }
      }
    }
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

  void DoMove(List<TurnAction> actions)
  {
    TeamRoleMap<Character> targetPoses = new TeamRoleMap<Character>();
    targetPoses = ObjectExtensions.Clone(ServerGameState.characters);
    foreach (var action in actions)
    {
      var character = targetPoses.GetItem(action.team, action.role);
      var newPos = new Vector2(character.x, character.y);
      if (!character.isScared)
        newPos += action.direction.ToDirectionVector();
      else
        character.isScared = false;

      character.x = Math.Max(Math.Min((int)newPos.X, mapInfo.tiles.Count - 1), 0);
      character.y = Math.Max(Math.Min((int)newPos.Y, mapInfo.tiles[character.x].Count - 1), 0);

      if (!IsInImpassableTile(ServerGameState, character))
      {
        targetPoses.SetItem(action.team, action.role, character);
      }
    }

    CancelMovementForCounterRolesSwapingPlaces(targetPoses, actions);
    ServerGameState.characters = targetPoses;
  }

  void CancelMovementForCounterRolesSwapingPlaces(TeamRoleMap<Character> targetPoses, List<TurnAction> actions)
  {
    if (AreSwapingPlaces(targetPoses, Team.Red, CharacterRole.Worm, Team.Blue, CharacterRole.Planter))
    {
      targetPoses.ReplaceWithItemFrom(ServerGameState.characters, Team.Red, CharacterRole.Worm);
      targetPoses.SetItem(Team.Red, CharacterRole.Worm, CancelCharacterAction(targetPoses.GetItem(Team.Red, CharacterRole.Worm)));
    }
    else if (AreSwapingPlaces(targetPoses, Team.Red, CharacterRole.Worm, Team.Blue, CharacterRole.Harvester))
    {
      targetPoses.ReplaceWithItemFrom(ServerGameState.characters, Team.Blue, CharacterRole.Harvester);
      targetPoses.SetItem(Team.Blue, CharacterRole.Harvester, CancelCharacterAction(targetPoses.GetItem(Team.Blue, CharacterRole.Harvester)));
    }

    if (AreSwapingPlaces(targetPoses, Team.Blue, CharacterRole.Worm, Team.Red, CharacterRole.Planter))
    {
      targetPoses.ReplaceWithItemFrom(ServerGameState.characters, Team.Blue, CharacterRole.Worm);
      targetPoses.SetItem(Team.Blue, CharacterRole.Worm, CancelCharacterAction(targetPoses.GetItem(Team.Blue, CharacterRole.Worm)));
    }
    else if (AreSwapingPlaces(targetPoses, Team.Blue, CharacterRole.Worm, Team.Red, CharacterRole.Harvester))
    {
      targetPoses.ReplaceWithItemFrom(ServerGameState.characters, Team.Red, CharacterRole.Harvester);
      targetPoses.SetItem(Team.Red, CharacterRole.Harvester, CancelCharacterAction(targetPoses.GetItem(Team.Red, CharacterRole.Harvester)));
    }
  }

  Character CancelCharacterAction(Character character)
  {
    character.cancelAction = true;
    return character;
  }

  bool AreSwapingPlaces(TeamRoleMap<Character> targetPoses, Team char1Team, CharacterRole char1Role, Team char2Team, CharacterRole char2Role)
  {
    return targetPoses.GetItem(char1Team, char1Role).DistanceTo(ServerGameState.characters.GetItem(char2Team, char2Role)) == 0
          && targetPoses.GetItem(char2Team, char2Role).DistanceTo(ServerGameState.characters.GetItem(char1Team, char1Role)) == 0;
  }

  void DoCatchWorm(ServerGameState serverGameState)
  {
    foreach (var planter in serverGameState.characters.GetItemsBy(CharacterRole.Planter))
    {
      foreach (var worm in serverGameState.characters.GetItemsBy(CharacterRole.Worm))
      {
        if (planter.team == worm.team) continue;
        if (worm.DistanceTo(planter) != 0) continue;

        var newWormState = worm;
        var newPlanterState = planter;

        var wormNewPosition = mapInfo.startingPositions.GetItem(worm.team, worm.characterRole);
        newWormState.x = (int)wormNewPosition.X;
        newWormState.y = (int)wormNewPosition.Y;
        serverGameState.characters.SetItem(worm.team, worm.characterRole, newWormState);

        newPlanterState.numWormCaught++;
        serverGameState.characters.SetItem(planter.team, planter.characterRole, newPlanterState);
      }
    }
  }

  void DoScareHarvester(ServerGameState serverGameState)
  {
    foreach (var worm in serverGameState.characters.GetItemsBy(CharacterRole.Worm))
    {
      foreach (var harvester in serverGameState.characters.GetItemsBy(CharacterRole.Harvester))
      {
        if (worm.team == harvester.team) continue;
        if (harvester.DistanceTo(worm) != 0) continue;

        var newHarvesterState = harvester;
        var newWormState = worm;

        newHarvesterState.isScared = true;
        serverGameState.characters.SetItem(harvester.team, harvester.characterRole, newHarvesterState);

        newWormState.numHarvesterScared++;
        serverGameState.characters.SetItem(worm.team, worm.characterRole, newWormState);
      }
    }
  }

  void DoPlantTree(ServerGameState serverGameState)
  {
    foreach (var planter in serverGameState.characters.GetItemsBy(CharacterRole.Planter))
    {
      var currentTile = serverGameState.map[planter.x][planter.y];
      if (currentTile.type == TileType.EMPTY)
      {
        currentTile.type = gameRule.FruitTileTypeForTeam(planter.team);
        serverGameState.map[planter.x][planter.y] = currentTile;

        var newPlanterState = planter;
        newPlanterState.numTreePlanted++;
        serverGameState.characters.SetItem(planter.team, planter.characterRole, newPlanterState);
      }
    }
  }

  void DoHarvest(ServerGameState serverGameState)
  {
    var harvesters = serverGameState.characters.GetItemsBy(CharacterRole.Harvester);
    foreach (var harvester in harvesters)
    {
      if (harvester.fruitCarrying >= gameRule.harvesterMaxCapacity) continue;

      var currentTile = serverGameState.map[harvester.x][harvester.y];
      var teamFruitType = gameRule.FruitTileTypeForTeam(harvester.team);

      if (IsTileRipeHarvestableFruit(currentTile, teamFruitType))
      {
        currentTile.growState = 1;
        serverGameState.map[harvester.x][harvester.y] = currentTile;

        var newHarvesterState = harvester;
        var fruitValue = gameRule.fruitScoreValues[currentTile.type];

        if (currentTile.type == TileType.WILDBERRY)
        {
          bool enemyHarvesterAtSamePlace = harvesters.Any(h => h.team != harvester.team && h.x == harvester.x && h.y == harvester.y);
          if (enemyHarvesterAtSamePlace) fruitValue /= 2;
        }

        newHarvesterState.fruitCarrying += fruitValue;
        newHarvesterState.numFruitHarvested += fruitValue;
        serverGameState.characters.SetItem(harvester.team, harvester.characterRole, newHarvesterState);
      }
    }
  }

  bool IsTileRipeHarvestableFruit(Tile tile, TileType teamFruitType)
  {
    return (tile.type == teamFruitType && tile.growState == gameRule.plantFruitTime)
      || (tile.type == TileType.WILDBERRY && tile.growState == gameRule.wildberryFruitTime);
  }

  void DoGetPoint(ServerGameState serverGameState)
  {
    foreach (var harvester in serverGameState.characters.GetItemsBy(CharacterRole.Harvester))
    {
      var currentTile = serverGameState.map[harvester.x][harvester.y];
      var scoreTileType = gameRule.ScoreTileTypeForTeam(harvester.team);

      if (currentTile.type == scoreTileType)
      {
        if (harvester.team == Team.Red) serverGameState.redScore += harvester.fruitCarrying;
        else serverGameState.blueScore += harvester.fruitCarrying;

        var newHarvesterState = harvester;
        newHarvesterState.numFruitDelivered += harvester.fruitCarrying;
        newHarvesterState.fruitCarrying = 0;
        serverGameState.characters.SetItem(harvester.team, harvester.characterRole, newHarvesterState);
      }
    }
  }

  void DoDestroyPlant(ServerGameState serverGameState)
  {
    foreach (var worm in serverGameState.characters.GetItemsBy(CharacterRole.Worm))
    {
      var currentTile = serverGameState.map[worm.x][worm.y];
      var destroyablePlantTypes = gameRule.WormDestroyTileTypeForTeam(worm.team);
      if (destroyablePlantTypes.Contains(currentTile.type))
      {
        currentTile.type = TileType.EMPTY;
        currentTile.growState = 0;
        serverGameState.map[worm.x][worm.y] = currentTile;

        var newWormState = worm;
        newWormState.numTreeDestroyed++;
        serverGameState.characters.SetItem(worm.team, worm.characterRole, newWormState);
      }
    }
  }

  void DoGrowPlant(ServerGameState serverGameState)
  {
    for (int row = 0; row < serverGameState.map.Count; row++)
    {
      for (int col = 0; col < serverGameState.map[row].Count; col++)
      {
        var tile = serverGameState.map[row][col];

        if (IsUnripePlant(tile))
        {
          tile.growState++;
          serverGameState.map[row][col] = tile;
        }
      }
    }
  }

  bool IsUnripePlant(Tile tile)
  {
    return (tile.type == TileType.WILDBERRY && tile.growState < gameRule.wildberryFruitTime)
    || ((tile.type == TileType.PUMPKIN || tile.type == TileType.TOMATO) && tile.growState < gameRule.plantFruitTime);
  }

  bool IsInImpassableTile(ServerGameState serverGameState, Character character)
  {
    return character.DistanceTo(serverGameState.map[character.x][character.y]) == 0 && serverGameState.map[character.x][character.y].type == TileType.IMPASSABLE;
  }

  ServerGameState InitializeCharacters(ServerGameState gameState, MapInfo mapInfo)
  {
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        var pos = mapInfo.startingPositions.GetItem(team, role);
        Character character = new Character((int)pos.X, (int)pos.Y, team, role);
        gameState.characters.SetItem(team, role, character);
      }
    }
    return gameState;
  }

  void AssignCharacterToControllers(ServerGameState gameState, TeamRoleMap<ICharacterController> controllers)
  {
    foreach (var character in gameState.characters)
    {
      controllers.GetItem(character.team, character.characterRole).Character = character;
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

  bool IsTileTypeAlwaysVisible(TileType type) => type == TileType.RED_BOX || type == TileType.BLUE_BOX || type == TileType.RED_ROCK || type == TileType.BLUE_ROCK;
}