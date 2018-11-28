using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System;

public class GameLogic
{
  public ServerGameState ServerGameState { get; private set; }
  List<ICharacterController> controllers = new List<ICharacterController>();

  /// <param name="replayGameState">Initial game state of a replay. Leave null to play new game.</param>
  public GameLogic(ServerGameState replayGameState = null)
  {
    if (replayGameState == null)
    {
      ServerGameState = new ServerGameState();

      InitializeCharacters(ServerGameState);
      InitializeMap(ServerGameState);
    }
    else ServerGameState = replayGameState;
  }

  /// <summary>Play a game from beginning to end</summary>
  /// <param name="recorder">The recorder used to log replay</param>
  public async Task StartGame(
    ICharacterController redPlanterController,
    ICharacterController redHarvesterController,
    ICharacterController redWormController,
    ICharacterController bluePlanterController,
    ICharacterController blueHarvesterController,
    ICharacterController blueWormController,
    IReplayRecorder recorder = null
  )
  {
    recorder?.LogGameState(ServerGameState);

    controllers.Add(redPlanterController);
    controllers.Add(redHarvesterController);
    controllers.Add(redWormController);
    controllers.Add(bluePlanterController);
    controllers.Add(blueHarvesterController);
    controllers.Add(blueWormController);

    StartGameForTeam(
      ServerGameState.GameStateForTeam(Team.Red),
      redPlanterController,
      redHarvesterController,
      redWormController
    );

    StartGameForTeam(
      ServerGameState.GameStateForTeam(Team.Blue),
      bluePlanterController,
      blueHarvesterController,
      blueWormController
    );

    while (ServerGameState.turn < GameConfigs.GAME_LENGTH)
    {
      await PlayNextTurn(recorder);
    }

    DoEnd(recorder);
  }

  void StartGameForTeam(GameState gameState, ICharacterController planterController, ICharacterController harvesterController, ICharacterController wormController)
  {
    foreach (var character in gameState.allies)
    {
      ICharacterController controller = null;
      if (character.characterRole == CharacterRole.Planter) controller = planterController;
      else if (character.characterRole == CharacterRole.Harvester) controller = harvesterController;
      else if (character.characterRole == CharacterRole.Worm) controller = wormController;
      else throw new System.Exception($"Unknown role {character.characterRole}!");

      StartGameForCharacter(gameState, controller, character);
    }
  }

  void StartGameForCharacter(GameState gameState, ICharacterController controller, Character character)
  {
    controller.Character = character;
    controller.DoStart(gameState);
  }

  async Task PlayNextTurn(IReplayRecorder recorder)
  {
    var redTeamGameState = ServerGameState.GameStateForTeam(Team.Red);
    var blueTeamGameState = ServerGameState.GameStateForTeam(Team.Blue);
    List<TurnAction> actions = new List<TurnAction>();

    foreach (var controller in controllers)
    {
      var result = await controller.DoTurn(
        controller.Character.team == Team.Red ? redTeamGameState : blueTeamGameState
      );
      actions.Add(new TurnAction(
        controller.Character.team,
        controller.Character.characterRole,
        result,
        controller.IsTimedOut,
        controller.IsCrashed
      ));
    }

    ExecuteTurn(actions);
    recorder?.LogTurn(actions);
  }

  /// <summary>Progress the game state with given actions</summary>
  public void ExecuteTurn(List<TurnAction> actions)
  {
    DoMove(actions);
    // TODO Planter catch Worm
    // TODO Worm scares Harvester
    // TODO Worm destroy Plant
    // TODO Planter plant
    // TODO Harvester harvest
    // TODO Harvester score
    // TODO Plants and Wildberries inc growState
    ServerGameState.turn++;
  }

  void DoEnd(IReplayRecorder recorder)
  {
    recorder?.LogEndGame(ServerGameState);
  }

  void DoMove(List<TurnAction> actions)
  {
    Dictionary<Team, Dictionary<CharacterRole, Character>> targetPoses = new Dictionary<Team, Dictionary<CharacterRole, Character>>();
    foreach (var action in actions)
    {
      var character = ServerGameState.characters[action.team][action.role];
      var newPos = new Vector2(character.x, character.y) + action.direction.ToDirectionVector();
      character.x = Math.Max(Math.Min((int)newPos.X, GameConfigs.MAP_WIDTH - 1), 0);
      character.y = Math.Max(Math.Min((int)newPos.Y, GameConfigs.MAP_HEIGHT - 1), 0);
      targetPoses[action.team][action.role] = character;
    }

    // Cancel movement on counter roles swaping places
    if (AreSwapingPlaces(targetPoses, Team.Red, CharacterRole.Worm, Team.Blue, CharacterRole.Planter))
    {
      targetPoses[Team.Red][CharacterRole.Worm] = ServerGameState.characters[Team.Red][CharacterRole.Worm];
    }
    else if (AreSwapingPlaces(targetPoses, Team.Red, CharacterRole.Worm, Team.Blue, CharacterRole.Harvester))
    {
      targetPoses[Team.Blue][CharacterRole.Harvester] = ServerGameState.characters[Team.Blue][CharacterRole.Harvester];
    }

    if (AreSwapingPlaces(targetPoses, Team.Blue, CharacterRole.Worm, Team.Red, CharacterRole.Planter))
    {
      targetPoses[Team.Blue][CharacterRole.Worm] = ServerGameState.characters[Team.Blue][CharacterRole.Worm];
    }
    else if (AreSwapingPlaces(targetPoses, Team.Blue, CharacterRole.Worm, Team.Red, CharacterRole.Harvester))
    {
      targetPoses[Team.Red][CharacterRole.Harvester] = ServerGameState.characters[Team.Red][CharacterRole.Harvester];
    }

    ServerGameState.characters = targetPoses;
  }

  bool AreSwapingPlaces(Dictionary<Team, Dictionary<CharacterRole, Character>> targetPoses, Team char1Team, CharacterRole char1Role, Team char2Team, CharacterRole char2Role)
  {
    return targetPoses[char1Team][char1Role].DistanceTo(ServerGameState.characters[char2Team][char2Role]) == 0
          && targetPoses[char2Team][char2Role].DistanceTo(ServerGameState.characters[char1Team][char1Role]) == 0;
  }

  ServerGameState InitializeCharacters(ServerGameState gameState)
  {
    InitializeTeam(Team.Red, gameState, GameConfigs.RED_STARTING_POSES);
    InitializeTeam(Team.Blue, gameState, GameConfigs.BLUE_STARTING_POSES);

    return gameState;
  }

  void InitializeTeam(Team team, ServerGameState gameState, List<Vector2> startingPoses)
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
      gameState.characters[team][(CharacterRole)i] = character;
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