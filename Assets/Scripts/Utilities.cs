using UnityEngine;

public class Utilities
{
  public static Color SetColorAlpha(Color color, float a)
  {
    return new Color(color.r, color.g, color.b, a);
  }
}