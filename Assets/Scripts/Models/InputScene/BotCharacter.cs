using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
public class BotCharacter : ICharacterController
{
  public MapInfo mapInfo;
  public IInputSceneUI uiManager;
  private bool isTimedOut = false;
  private bool isCrashed = false;
  public Character Character { get; set; }
  public bool IsTimedOut
  {
    get
    {
      return isTimedOut;
    }
  }

  public bool IsCrashed
  {
    get
    {
      return isCrashed;
    }
  }

  private Func<GameState, GameConfig, string> DoThink;
  private List<List<Node>> listNodes = new List<List<Node>>();
  private Node nextNode;

  public BotCharacter()
  {
    switch (Character.characterRole)
    {
      case CharacterRole.Planter:
        DoThink = DoPlanterThink;
        break;
      case CharacterRole.Harvester:
        DoThink = DoHarvesterThink;
        break;
      case CharacterRole.Worm:
        DoThink = DoWormThink;
        break;
      default:
        throw new System.Exception($"Unknown Character role: {Character.characterRole.ToString()}");
    }
  }

  public async Task DoStart(GameState gameState, GameConfig gameRule)
  {
    // TODO use the same timeout config as DoTurn
    // TODO convert map to grid node
    foreach (var row in gameState.map)
    {
      List<Node> nodeRow = new List<Node>();
      foreach (var col in row)
      {
        Node node = new Node(col.x, col.y, col.type != TileType.IMPASSABLE);
        nodeRow.Add(node);
      }
      listNodes.Add(nodeRow);
    }
  }

  public async Task<string> DoTurn(GameState gameState, GameConfig gameRule)
  {
    UpdateCharacter(gameState);

    var ct = new CancellationTokenSource(100);
    var tcs = new TaskCompletionSource<bool>();
    ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

    string result = Directions.STAY;
    await Task.Factory.StartNew(() => DoThink?.Invoke(gameState, gameRule)).ContinueWith((task) =>
    {
      if (task.IsFaulted)
      {
        isCrashed = true;
      }
      else if (task.IsCanceled || ct.IsCancellationRequested)
      {
        isTimedOut = true;
      }
      else
      {
        ct.Cancel();
        result = task.Result;
      }
    });
    return result;
  }

  void UpdateCharacter(GameState gameState)
  {
    foreach (var ally in gameState.allies)
    {
      if (ally.characterRole == Character.characterRole)
      {
        Character = ally;
      }
    }
  }

  public string DoPlanterThink(GameState gameState, GameConfig gameRule)
  {
    bool needChaseWorm = false;
    Vector2 wormPos = Vector2.Zero;
    foreach (var enemy in gameState.enemies)
    {
      if (enemy.characterRole == CharacterRole.Worm)
      {
        needChaseWorm = true;
        wormPos = new Vector2(enemy.x, enemy.y);
        break;
      }
    }
    if (needChaseWorm)
    {
      nextNode = MoveToPos(new Vector2(Character.x, Character.y), wormPos);
      return DirectionStringExtension.ToDirectionString(new Vector2(nextNode.x, nextNode.y) - new Vector2(Character.x, Character.y));
    }
    else
    {
      return GetPreferredDirection(gameState, TileType.EMPTY);
    }
  }

  public string DoHarvesterThink(GameState gameState, GameConfig gameRule)
  {
    bool needComeBack = false;
    if (Character.fruitCarrying >= 5) needComeBack = true;

    if (needComeBack)
    {
      nextNode = MoveToPos(new Vector2(Character.x, Character.y), mapInfo.startingPositions.GetItem(Character.team, CharacterRole.Harvester));
      return DirectionStringExtension.ToDirectionString(new Vector2(nextNode.x, nextNode.y) - new Vector2(Character.x, Character.y));
    }
    else
    {
      var closestHarvestTree = GetClosestTileType(gameState, Character.team == 0 ? TileType.TOMATO : TileType.PUMPKIN, gameRule.plantFruitTime);
      var closestHarvestWildBerry = GetClosestTileType(gameState, TileType.WILDBERRY, gameRule.wildberryFruitTime);
      if (closestHarvestTree.type != TileType.EMPTY)
      {
        nextNode = MoveToPos(new Vector2(Character.x, Character.y), new Vector2(closestHarvestTree.x, closestHarvestTree.y));
        return DirectionStringExtension.ToDirectionString(new Vector2(nextNode.x, nextNode.y) - new Vector2(Character.x, Character.y));
      }
      else if (closestHarvestWildBerry.type != TileType.EMPTY)
      {
        nextNode = MoveToPos(new Vector2(Character.x, Character.y), new Vector2(closestHarvestWildBerry.x, closestHarvestWildBerry.y));
        return DirectionStringExtension.ToDirectionString(new Vector2(nextNode.x, nextNode.y) - new Vector2(Character.x, Character.y));
      }
      else return GetPreferredDirection(gameState, Character.team == Team.Blue ? TileType.PUMPKIN : TileType.TOMATO);
    }
  }

  public string DoWormThink(GameState gameState, GameConfig gameRule)
  {
    Vector2 harvesterPos = Vector2.Zero;
    bool needChase = false;

    var enemyClosestTree = GetClosestTileType(gameState, Character.team == 0 ? TileType.PUMPKIN : TileType.TOMATO);
    if (enemyClosestTree.type != TileType.EMPTY)
    {
      nextNode = MoveToPos(new Vector2(Character.x, Character.y), new Vector2(enemyClosestTree.x, enemyClosestTree.y));
      return DirectionStringExtension.ToDirectionString(new Vector2(nextNode.x, nextNode.y) - new Vector2(Character.x, Character.y));
    }

    foreach (var enemy in gameState.enemies)
    {
      if (enemy.characterRole == CharacterRole.Harvester)
      {
        needChase = true;
        harvesterPos = new Vector2(enemy.x, enemy.y);
        break;
      }
    }

    if (needChase)
    {
      if (Character.x == harvesterPos.X && Character.y == harvesterPos.Y) return Directions.STAY;
      else
      {
        nextNode = MoveToPos(new Vector2(Character.x, Character.y), harvesterPos);
        return DirectionStringExtension.ToDirectionString(new Vector2(nextNode.x, nextNode.y) - new Vector2(Character.x, Character.y));
      }
    }
    else return GetPreferredDirection(gameState, Character.team == Team.Blue ? TileType.PUMPKIN : TileType.TOMATO);
  }
  //---------------------------------------- Random path ----------------------------------------------
  Tile GetClosestTileType(GameState gameState, TileType tileType, int growState = 0)
  {
    int maxDistance = int.MinValue;
    Tile targetTile = new Tile();
    targetTile.type = TileType.EMPTY;
    foreach (var row in gameState.map)
    {
      foreach (var col in row)
      {
        if (col.type == tileType && col.DistanceTo(this.Character) > maxDistance)
        {
          if (growState != 0 && col.growState != growState) continue;
          maxDistance = col.DistanceTo(this.Character);
          targetTile = col;
          break;
        }
      }
    }
    return targetTile;
  }
  string GetPreferredDirection(GameState gameState, TileType preferredTileType)
  {
    var neighbourTiles = GetNeighbourTiles(gameState.map[Character.x][Character.y], gameState);
    Tile preferredTile = gameState.map[Character.x][Character.y];
    foreach (var tile in neighbourTiles)
    {
      if (tile.type == preferredTileType)
      {
        preferredTile = tile;
        break;
      }
    }
    if (preferredTile != gameState.map[Character.x][Character.y]) return DirectionStringExtension.ToDirectionString(new Vector2(preferredTile.x, preferredTile.y) - new Vector2(Character.x, Character.y));
    else return GetRandomDirection();
  }

  string GetRandomDirection()
  {
    Vector2 direction = Vector2.Zero;
    var rnd = RandomRange(0, 5);
    switch (rnd)
    {
      case 0: //up
        direction = new Vector2(0, 1);
        break;
      case 1: //down
        direction = new Vector2(0, -1);
        break;
      case 2: //left
        direction = new Vector2(-1, 0);
        break;
      case 3: //right
        direction = new Vector2(1, 0);
        break;
      default:
        direction = Vector2.Zero;
        break;
    }
    if (Character.x + direction.X >= mapInfo.tiles.Count
    || Character.x + direction.X < 0
    || Character.y + direction.Y >= mapInfo.tiles[0].Count
    || Character.y + direction.Y < 0) return GetRandomDirection();
    else return DirectionStringExtension.ToDirectionString(direction);
  }

  int RandomRange(int min, int max)
  {
    Random r = new Random();
    return r.Next(min, max);
  }
  //---------------------------------------- A* path finding -------------------------------------------

  public Node MoveToPos(Vector2 startPos, Vector2 targetPos)
  {
    if (startPos == targetPos) return listNodes[(int)targetPos.X][(int)targetPos.Y];
    Node startNode = listNodes[(int)startPos.X][(int)startPos.Y];
    Node targetNode = listNodes[(int)targetPos.X][(int)targetPos.Y];

    List<Node> openSet = new List<Node>();
    HashSet<Node> closedSet = new HashSet<Node>();
    openSet.Add(startNode);

    while (openSet.Count > 0)
    {
      Node currentNode = openSet[0];
      for (int i = 0; i < openSet.Count; i++)
      {
        if (openSet[i].fCost < currentNode.fCost
        || openSet[i].fCost == currentNode.fCost
        && openSet[i].hCost < currentNode.hCost)
          currentNode = openSet[i];
      }

      openSet.Remove(currentNode);
      closedSet.Add(currentNode);

      if (currentNode == targetNode)
      {
        var path = RetracePath(startNode, targetNode);
        return path[0];
      }

      foreach (Node neighbour in GetNeighbourNodes(currentNode))
      {
        if (!neighbour.isPassable || closedSet.Contains(neighbour)) continue;
        int newMovementCostToNeighbour = currentNode.gCost + currentNode.GetDistance(neighbour);
        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
        {
          neighbour.gCost = newMovementCostToNeighbour;
          neighbour.hCost = neighbour.GetDistance(targetNode);
          neighbour.parentNode = currentNode;

          if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
        }
      }
    }
    return startNode;
  }

  List<Node> RetracePath(Node startNode, Node endNode)
  {
    List<Node> path = new List<Node>();
    Node currentNode = endNode;

    while (currentNode != startNode)
    {
      path.Add(currentNode);
      currentNode = currentNode.parentNode;
    }
    path.Reverse();
    return path;
  }

  List<Node> GetNeighbourNodes(Node currentNode)
  {
    List<Node> neighbours = new List<Node>();
    for (int i = -1; i < 2; i++)
    {
      if (i == 0) continue;
      if (currentNode.x + i >= 0 && currentNode.x + i < mapInfo.tiles.Count) neighbours.Add(listNodes[currentNode.x + i][currentNode.y]);
      if (currentNode.y + i >= 0 && currentNode.y + i < mapInfo.tiles[0].Count) neighbours.Add(listNodes[currentNode.x][currentNode.y + i]);
    }
    return neighbours;
  }

  List<Tile> GetNeighbourTiles(Tile currentTile, GameState gameState)
  {
    List<Tile> neighbours = new List<Tile>();
    for (int i = -1; i < 2; i++)
    {
      if (i == 0) continue;
      if (currentTile.x + i >= 0 && currentTile.x + i < mapInfo.tiles.Count) neighbours.Add(gameState.map[currentTile.x + i][currentTile.y]);
      if (currentTile.y + i >= 0 && currentTile.y + i < mapInfo.tiles[0].Count) neighbours.Add(gameState.map[currentTile.x][currentTile.y + i]);
    }
    return neighbours;
  }
}

public class Node
{
  public int x;
  public int y;
  public bool isPassable;
  public Node parentNode;
  public int gCost;
  public int hCost;
  public int fCost
  {
    get
    {
      return gCost + hCost;
    }
  }
  public Node(int x, int y, bool isPassable)
  {
    this.x = x;
    this.y = y;
    this.isPassable = isPassable;
  }

  public int GetDistance(Node targetNode)
  {
    int dstX = UnityEngine.Mathf.Abs(x - targetNode.x);
    int dstY = UnityEngine.Mathf.Abs(y - targetNode.y);
    if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
    else return 14 * dstX + 10 * (dstY - dstX);
    // return dstX + dstY;
  }
}