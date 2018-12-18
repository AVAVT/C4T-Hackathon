using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapDisplayData : MapInfo
{
  public string mapName = "Default";
  public Sprite mapPreview;
  public Texture2D mapTileData;
  public TileTypeColorCode typeColors;

  public MapInfo ToMapInfo(GameConfig gameRule)
  {
    var mapInfo = new MapInfo();

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
          mapInfo.startingPositions.SetItem(Team.Red, CharacterRole.Planter, new System.Numerics.Vector2(x, y));
          mapInfo.startingPositions.SetItem(Team.Red, CharacterRole.Harvester, new System.Numerics.Vector2(x, y));
        }
        else if (tileType == TileType.BLUE_BOX)
        {
          mapInfo.startingPositions.SetItem(Team.Blue, CharacterRole.Planter, new System.Numerics.Vector2(x, y));
          mapInfo.startingPositions.SetItem(Team.Blue, CharacterRole.Harvester, new System.Numerics.Vector2(x, y));
        }
        else if (tileType == TileType.RED_ROCK)
        {
          mapInfo.startingPositions.SetItem(Team.Red, CharacterRole.Worm, new System.Numerics.Vector2(x, y));
        }
        else if (tileType == TileType.BLUE_ROCK)
        {
          mapInfo.startingPositions.SetItem(Team.Blue, CharacterRole.Worm, new System.Numerics.Vector2(x, y));
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