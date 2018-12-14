using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class MapBase
{
  public string MAP_NAME;
  public int MAP_WIDTH;
  public int MAP_HEIGHT;
  public Vector2 RED_BOX_POS;
  public Vector2 BLUE_BOX_POS;
  public Vector2 RED_ROCK_POS;
  public Vector2 BLUE_ROCK_POS;
  public List<Vector2> WATER_POSES;
  public List<Vector2> WILDBERRY_POSES;
  public List<Vector2> RED_STARTING_POSES;
  public List<Vector2> BLUE_STARTING_POSES;
}