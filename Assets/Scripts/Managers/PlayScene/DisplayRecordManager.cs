using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GracesGames.SimpleFileBrowser.Scripts;
using Newtonsoft.Json;
using UnityEngine;

public class DisplayRecordManager : MonoBehaviour
{
  private string logPath;
  [Header("File loader")]
  public GameObject FileBrowserPrefab;
  public string[] FileExtensions;
  public bool PortraitMode;

  [Header("Map")]
  [SerializeField] private Transform gridTransform;
  [SerializeField] private Sprite unknownSprite, impassableSprite, emptySprite, redBoxSprite, redRockSprite, blueBoxSprite, blueRockSprite;
  [SerializeField] private Sprite[] wildBerrySprite, tomatoSprite, pumpkinSprite;
  [SerializeField] private Vector2 gridSize, gridOffset;
  [SerializeField] private Vector2 cellSize;
  private int rows, cols;
  List<List<Tile>> mapInfo = new List<List<Tile>>();
  private Vector2 cellScale;
  private List<List<GameObject>> cellGOs = new List<List<GameObject>>();
  private List<RecordModel> logs = new List<RecordModel>();

  public PlayRecordUI uiManager;

  //--------------------------------- Read file --------------------------------------
  public void OpenFileBrowser()
  {
    GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
    fileBrowserObject.name = "FileBrowser";

    FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
    fileBrowserScript.SetupFileBrowser(PortraitMode ? ViewMode.Portrait : ViewMode.Landscape);

    fileBrowserScript.OpenFilePanel(FileExtensions);
    // Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
    fileBrowserScript.OnFileSelect += LoadFileUsingPath;
  }

  // Loads a file using a path
  private void LoadFileUsingPath(string path)
  {
    if (!String.IsNullOrEmpty(path))
    {
      logPath = path;
      logs = GetLogFromPath(logPath);
      mapInfo = logs[0].serverGameState.map;
      rows = logs[0].serverGameState.mapHeight;
      cols = logs[0].serverGameState.mapWidth;
    }
    else
    {
      Debug.Log("Invalid path given");
    }
  }

  //----------------------------------- Play record ----------------------------------
  public void PlayRecordLog()
  {
    if (logPath == null)
    {
      logPath = PlayerPrefs.GetString("LogPath");
      logs = GetLogFromPath(logPath);
      mapInfo = logs[0].serverGameState.map;
    }
    uiManager.LoadPanel.SetActive(false);
    InitGrid();
  }

  List<RecordModel> GetLogFromPath(string path)
  {
    string json = File.ReadAllText(path);
    return JsonConvert.DeserializeObject<List<RecordModel>>(json);
  }

  void InitGrid()
  {
    GameObject cellObject = new GameObject();
    cellObject.AddComponent<SpriteRenderer>();
    //get the new cell size -> adjust the size of the cells to fit the size of the grid
    Vector2 newCellSize = new Vector2(gridSize.x / (float)cols, gridSize.y / (float)rows);

    //Get the scales so you can scale the cells and change their size to fit the grid
    cellScale.x = newCellSize.x / cellSize.x;
    cellScale.y = newCellSize.y / cellSize.y;

    cellSize = newCellSize; //the size will be replaced by the new computed size, we just used cellSize for computing the scale

    cellObject.transform.localScale = new Vector2(cellScale.x, cellScale.y);

    //fix the cells to the grid by getting the half of the grid and cells add and minus experiment
    gridOffset.x = -(gridSize.x / 2) + cellSize.x / 2;
    gridOffset.y = -(gridSize.y / 2) + cellSize.y / 2;

    //fill the grid with cells by using Instantiate
    for (int col = 0; col < cols; col++)
    {
      var temp = new List<GameObject>();
      for (int row = rows - 1; row >= 0; row--)
      {
        //add the cell size so that no two cells will have the same x and y position
        Vector2 pos = new Vector2(col * cellSize.x + gridOffset.x + transform.position.x, row * cellSize.y + gridOffset.y + transform.position.y);

        //instantiate the game object, at position pos, with rotation set to identity
        GameObject cO = Instantiate(cellObject, pos, Quaternion.identity) as GameObject;
        cO.GetComponent<SpriteRenderer>().sprite = GetSpriteByTileType(mapInfo[col][rows - row - 1].type, mapInfo[col][rows - row - 1].growState);
        //set the parent of the cell to GRID so you can move the cells together with the grid;
        cO.transform.parent = gridTransform;
        temp.Add(cO);
      }
      cellGOs.Add(temp);
    }
    //destroy the object used to instantiate the cells
    Destroy(cellObject);
  }

  Sprite GetSpriteByTileType(TileType tileType, int growState)
  {
    switch (tileType)
    {
      case TileType.BLUE_BOX:
        return blueBoxSprite;
      case TileType.BLUE_ROCK:
        return blueRockSprite;
      case TileType.EMPTY:
        return emptySprite;
      case TileType.IMPASSABLE:
        return impassableSprite;
      case TileType.PUMPKIN:
        return pumpkinSprite[growState - 1];
      case TileType.RED_BOX:
        return redBoxSprite;
      case TileType.RED_ROCK:
        return redRockSprite;
      case TileType.TOMATO:
        return tomatoSprite[growState - 1];
      case TileType.UNKNOWN:
        return unknownSprite;
      case TileType.WILDBERRY:
        return wildBerrySprite[growState - 1];
      default:
        throw new System.Exception($"Unknown tile type: {tileType}");
    }
  }
}
