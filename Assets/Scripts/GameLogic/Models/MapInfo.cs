using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class MapInfo
{
  public TeamRoleMap<Vector2> startingPositions = new TeamRoleMap<Vector2>();
  public List<List<TileType>> tiles = new List<List<TileType>>();
}