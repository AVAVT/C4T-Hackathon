using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "TileTypeColorCode", menuName = "C4T-Hackathon/TileTypeColorCode", order = 0)]
public class TileTypeColorCode : SerializedScriptableObject
{
  public Dictionary<Color, TileType> values;
}