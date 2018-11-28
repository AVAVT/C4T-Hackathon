using System.Numerics;

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
  public static Vector2 ToDirectionVector(this string direction)
  {
    switch (direction)
    {
      case Directions.UP: return new Vector2(0, -1);
      case Directions.DOWN: return new Vector2(0, 1);
      case Directions.LEFT: return new Vector2(-1, 0);
      case Directions.RIGHT: return new Vector2(1, 0);
      case Directions.STAY: return Vector2.Zero;
      default: throw new System.Exception($"String '{direction}' is not a valid direction!");
    }
  }
}