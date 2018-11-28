using System.Collections.Generic;
using System;
using System.Numerics;

public struct Tile
{
  public int x;
  public int y;
  public TileType type;
  public int growState;
  public bool alwaysVisible;

  public Tile(int x, int y)
  {
    this.x = x;
    this.y = y;
    type = TileType.UNKNOWN;
    growState = 0;
    alwaysVisible = false;
  }

  public int DistanceTo(Character character)
  {
    return Math.Abs(x - character.x) + Math.Abs(y - character.y);
  }

  public int DistanceTo(Tile tile)
  {
    return Math.Abs(x - tile.x) + Math.Abs(y - tile.y);
  }

  public static bool operator ==(Tile tile, Vector2 pos)
  {
    return tile.x == (int)pos.X && tile.y == (int)pos.Y;
  }
  public static bool operator !=(Tile tile, Vector2 pos)
  {
    return tile.x != (int)pos.X || tile.y != (int)pos.Y;
  }

  public static bool operator ==(Tile tile, Tile tile2)
  {
    return tile.x == (int)tile2.x && tile.y == (int)tile2.y;
  }
  public static bool operator !=(Tile tile, Tile tile2)
  {
    return tile.x != (int)tile2.x || tile.y != (int)tile2.y;
  }
  public override bool Equals(object obj)
  {
    if (obj is Tile)
      return this == (Tile)obj;
    else if (obj is Vector2)
      return this == (Vector2)obj;
    else return false;
  }

  public override int GetHashCode()
  {
    return 26 * x + 3 * y;
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