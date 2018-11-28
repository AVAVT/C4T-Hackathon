using UnityEngine;

public class Directions
{
  public const string UP = "UP";
  public const string DOWN = "DOWN";
  public const string LEFT = "LEFT";
  public const string RIGHT = "RIGHT";
  public const string STAY = "STAY";
}

public static class DirectionStringExtension
{
  public static Vector2Int ToDirectionVector(this string direction)
  {
    switch (direction)
    {
      case Directions.UP: return Vector2Int.up;
      case Directions.DOWN: return Vector2Int.down;
      case Directions.LEFT: return Vector2Int.left;
      case Directions.RIGHT: return Vector2Int.right;
      case Directions.STAY: return Vector2Int.zero;
      default: throw new System.Exception($"String '{direction}' is not a valid direction!");
    }
  }
}