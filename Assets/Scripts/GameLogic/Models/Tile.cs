using System.Collections.Generic;
using UnityEngine;

public class Tile
{
  public Vector2Int position;
  public TileType type = TileType.UNKNOWN;
  public int growState = 0;

  public Tile(int x, int y)
  {
    position = new Vector2Int(x, y);
    type = TileType.UNKNOWN;
    growState = 0;
  }
}

public enum TileType
{
  UNKNOWN = 0,
  IMPASSABLE = 1,
  EMPTY = 2,
  WILDBERRY = 3,
  TOMATO = 4,
  PUMPKIN = 5
}