using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class MapInfo
{
  public Dictionary<Team, Dictionary<CharacterRole, Vector2>> startingPositions = new Dictionary<Team, Dictionary<CharacterRole, Vector2>>();
  public List<List<TileType>> tiles = new List<List<TileType>>();
}