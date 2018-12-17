using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapDisplayData : MapInfo
{
  public string mapName = "Default";
  public Sprite mapPreview;
  public Texture2D mapTileData;
  public TileTypeColorCode typeColors;

  public MapInfo ToMapInfo(GameRule gameRule)
  {
    var mapInfo = new MapInfo();

    foreach (var team in gameRule.availableTeams)
    {
      mapInfo.startingPositions[team] = new Dictionary<CharacterRole, System.Numerics.Vector2>();
    }

    for (int x = 0; x < mapTileData.width; x++)
    {
      mapInfo.tiles.Add(new List<TileType>());

      for (int y = 0; y < mapTileData.height; y++)
      {
        TileType tileType = TypeFromColor(mapTileData.GetPixel(x, y));
        mapInfo.tiles[x].Add(tileType);


        // TODO use C# 7
        if (tileType == TileType.RED_BOX)
        {
          mapInfo.startingPositions[Team.Red][CharacterRole.Planter] = new System.Numerics.Vector2(x, y);
          mapInfo.startingPositions[Team.Red][CharacterRole.Harvester] = new System.Numerics.Vector2(x, y);
        }
        else if (tileType == TileType.BLUE_BOX)
        {
          mapInfo.startingPositions[Team.Blue][CharacterRole.Planter] = new System.Numerics.Vector2(x, y);
          mapInfo.startingPositions[Team.Blue][CharacterRole.Harvester] = new System.Numerics.Vector2(x, y);
        }
        else if (tileType == TileType.RED_ROCK)
        {
          mapInfo.startingPositions[Team.Red][CharacterRole.Worm] = new System.Numerics.Vector2(x, y);
        }
        else if (tileType == TileType.BLUE_ROCK)
        {
          mapInfo.startingPositions[Team.Blue][CharacterRole.Worm] = new System.Numerics.Vector2(x, y);
        }
      }
    }

    return mapInfo;
  }

  TileType TypeFromColor(Color color)
  {
    return typeColors.values[color];
  }
}