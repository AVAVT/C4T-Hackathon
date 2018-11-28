using System.Collections.Generic;
using System.Numerics;

public static class GameConfigs
{
  public static readonly int MAP_WIDTH = 7;
  public static readonly int MAP_HEIGHT = 5;
  public static readonly int PLANT_FRUIT_TIME = 5;
  public static readonly int WILDBERRY_FRUIT_TIME = 10;
  public static readonly Vector2 RED_BOX_POS = Vector2.Zero;
  public static readonly Vector2 BLUE_BOX_POS = new Vector2(MAP_WIDTH - 1, MAP_HEIGHT - 1);
  public static readonly Vector2 RED_ROCK_POS = new Vector2(0, MAP_HEIGHT - 1);
  public static readonly Vector2 BLUE_ROCK_POS = new Vector2(MAP_WIDTH - 1, 0);
  public static readonly List<Vector2> WILDBERRY_POS = new List<Vector2>(){
    new Vector2(2,3),
    new Vector2(4,1)
  };
  public static readonly int SIGHT_DISTANCE = 2;
  public static readonly int GAME_LENGTH = 1000;
}