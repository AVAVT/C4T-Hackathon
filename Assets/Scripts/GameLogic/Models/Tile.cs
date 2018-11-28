using System.Collections.Generic;
using UnityEngine;

public struct Tile
{
  public Vector2Int position;
  public TileType type;
  public int growState;

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
  PUMPKIN = 5,
  RED_BOX = 6,
  BLUE_BOX = 7,
  RED_ROCK = 8,
  BLUE_ROCK = 9
}