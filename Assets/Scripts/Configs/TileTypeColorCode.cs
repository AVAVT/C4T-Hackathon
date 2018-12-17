using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeColorCode", menuName = "C4T-Hackathon/TileTypeColorCode", order = 0)]
public class TileTypeColorCode : ScriptableObject
{
  public Dictionary<Color, TileType> values;
}