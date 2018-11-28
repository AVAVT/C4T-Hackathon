using System.Collections.Generic;
using UnityEngine;

public static class GameConfigs
{
  public static readonly int MAP_WIDTH = 7;
  public static readonly int MAP_HEIGHT = 5;
  public static readonly int PLANT_FRUIT_TIME = 5;
  public static readonly int WILDBERRY_FRUIT_TIME = 10;
  public static readonly Vector2Int RED_BOX_POS = Vector2Int.zero;
  public static readonly Vector2Int BLUE_BOX_POS = new Vector2Int(MAP_WIDTH - 1, MAP_HEIGHT - 1);
  public static readonly Vector2Int RED_ROCK_POS = new Vector2Int(0, MAP_HEIGHT - 1);
  public static readonly Vector2Int BLUE_ROCK_POS = new Vector2Int(MAP_WIDTH - 1, 0);
  public static readonly List<Vector2Int> WILDBERRY_POS = new List<Vector2Int>(){
    new Vector2Int(2,3),
    new Vector2Int(4,1)
  };
  public static readonly int SIGHT_DISTANCE = 2;
  public static readonly int GAME_LENGTH = 1000;
}