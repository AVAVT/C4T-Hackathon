﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using DG.Tweening;

public class DisplayRecordManager : MonoBehaviour
{
  public IPlaySceneUI uiManager;
  private string logPath;

  [Header("Map")]
  [SerializeField] private Transform gridTransform;
  [SerializeField] private Sprite unknownSprite;
  [SerializeField] private Sprite impassableSprite;
  [SerializeField] private Sprite emptySprite;
  [SerializeField] private Sprite redBoxSprite;
  [SerializeField] private Sprite redRockSprite;
  [SerializeField] private Sprite blueBoxSprite;
  [SerializeField] private Sprite blueRockSprite;
  [SerializeField] private Sprite[] wildBerrySprite;
  [SerializeField] private Sprite[] tomatoSprite;
  [SerializeField] private Sprite[] pumpkinSprite;
  [SerializeField] private Vector2 gridSize, gridOffset;
  [SerializeField] private Vector2 cellSize;
  private int rows, cols;
  List<List<Tile>> mapInfo = new List<List<Tile>>();
  private Vector2 cellScale;
  private List<List<GameObject>> cellGOs = new List<List<GameObject>>();
  private List<RecordModel> logs = new List<RecordModel>();

  [Header("Characters")]
  [SerializeField] private Vector2 characterSize;
  [SerializeField] private GameObject redPlanterPrefab;
  [SerializeField] private GameObject redHarvesterPrefab;
  [SerializeField] private GameObject redWormPrefab;
  [SerializeField] private GameObject bluePlanterPrefab;
  [SerializeField] private GameObject blueHarvesterPrefab;
  [SerializeField] private GameObject blueWormPrefab;
  private Dictionary<Team, Dictionary<CharacterRole, GameObject>> characterGOs = new Dictionary<Team, Dictionary<CharacterRole, GameObject>>();
  private Dictionary<Team, Dictionary<CharacterRole, Character>> characters = new Dictionary<Team, Dictionary<CharacterRole, Character>>();
  private List<string> listNames = new List<string>();


  [Header("Playing records")]
  [SerializeField] private float turnTime;
  private float timeMultiplier;
  private PlayRecordState playRecordState = PlayRecordState.Stop;
  private int currentTurn = 0;

  void Awake()
  {
    ChangeGameSpeedButtonClick(1);
    uiManager = GetComponent<PlayRecordUI>();

    uiManager.PlayLog = PlayButtonClick;
    uiManager.StopLog = StopPlayRecord;
    uiManager.NextTurn = NextButtonClick;
    uiManager.PrevTurn = PrevButtonClick;
    uiManager.ChangeGameSpeed = ChangeGameSpeedButtonClick;
    uiManager.ChangeTurn = ChangeTurn;

    PrepareScene();
  }
  void GetLogFromPath(string path)
  {
    string json = File.ReadAllText(path);
    logs = JsonConvert.DeserializeObject<List<RecordModel>>(json);
    mapInfo = logs[currentTurn].serverGameState.map;
    rows = logs[currentTurn].serverGameState.mapHeight;
    cols = logs[currentTurn].serverGameState.mapWidth;
    characters = logs[currentTurn].serverGameState.characters;
  }

  List<string> GetPlayerNames()
  {
    List<string> listNames = new List<string>();
    for (int i = 0; i < 6; i++)
    {
      if (PlayerPrefs.GetString($"Player{i}Name", $"Player {i}") == "") listNames.Add($"Player {i}");
      else listNames.Add(PlayerPrefs.GetString($"Player{i}Name", $"Player {i}"));
    }
    return listNames;
  }
  //----------------------------------- Play record ----------------------------------
  public void PrepareScene()
  {
    if (logPath == null)
    {
      logPath = PlayerPrefs.GetString("LogPath");
      GetLogFromPath(logPath);
    }

    listNames = GetPlayerNames();
    uiManager.DisplayGameInfo(listNames, logs);
    uiManager.DisplayTurnInfo(currentTurn, logs[currentTurn].serverGameState.blueScore, logs[currentTurn].serverGameState.redScore);
    InitGrid();
    InitCharacter();
  }

  IEnumerator PlayLogs(bool isContinue = false)
  {
    playRecordState = PlayRecordState.Playing;
    if (!isContinue) currentTurn++;
    while (currentTurn < logs.Count)
    {
      if (currentTurn == logs.Count - 1) uiManager.ToggleResult(true);
      uiManager.DisplayTurnInfo(currentTurn, logs[currentTurn].serverGameState.blueScore, logs[currentTurn].serverGameState.redScore);
      yield return ChangeCharacter(currentTurn);
      currentTurn++;
    }
  }

  IEnumerator ChangeMap(int currentTurn)
  {
    Debug.Log("Change map");
    var currentMap = logs[currentTurn].serverGameState.map;
    for (int row = 0; row < logs[currentTurn].serverGameState.mapWidth; row++)
    {
      for (int col = 0; col < logs[currentTurn].serverGameState.mapHeight; col++)
      {
        if (mapInfo[row][col].type != currentMap[row][col].type)
        {
          mapInfo[row][col] = currentMap[row][col];
          cellGOs[row][col].GetComponent<SpriteRenderer>().sprite = GetSpriteByTileType(mapInfo[row][col].type, mapInfo[row][col].growState);
          continue;
        }
        if (currentTurn == 0)
        {
          if (mapInfo[row][col].type == TileType.TOMATO || mapInfo[row][col].type == TileType.PUMPKIN)
          {
            var tempTile = mapInfo[row][col];
            tempTile.type = TileType.EMPTY;
            mapInfo[row][col] = tempTile;
            cellGOs[row][col].GetComponent<SpriteRenderer>().sprite = GetSpriteByTileType(mapInfo[row][col].type, mapInfo[row][col].growState);
          }
          else if (mapInfo[row][col].type == TileType.WILDBERRY)
          {
            var tempTile = mapInfo[row][col];
            tempTile.growState = 10;
            mapInfo[row][col] = tempTile;
            cellGOs[row][col].GetComponent<SpriteRenderer>().sprite = GetSpriteByTileType(mapInfo[row][col].type, mapInfo[row][col].growState);
          }
        }
      }
    }
    yield return null;
  }

  IEnumerator ChangeCharacter(int currentTurn)
  {
    bool isFinishAnimation = false;
    var currentActions = logs[currentTurn].actions;
    if (currentActions != null)
    {
      foreach (var action in currentActions)
      {
        var currentCharacter = characters[action.team][action.role];
        if (action.timedOut)
        {
          //TODO: Show time out
          Debug.Log($"AI of {currentCharacter.characterRole} - team {currentCharacter.team} is time out!");
          isFinishAnimation = true;
        }
        else if (action.crashed)
        {
          //TODO: Show is crashed
          Debug.Log($"AI of {currentCharacter.characterRole} - team {currentCharacter.team} is crashed!");
          isFinishAnimation = true;
        }
        else
        {
          if (playRecordState == PlayRecordState.Playing)
          {
            var direction = DirectionStringExtension.ToDirectionVector(action.direction);
            if (direction.X != 0 || direction.Y != 0 && IsValidDirection(currentCharacter.x, currentCharacter.y, (int)direction.X, (int)direction.Y))
            {
              characterGOs[currentCharacter.team][currentCharacter.characterRole].transform
                .DOMove(cellGOs[currentCharacter.x + (int)direction.X][currentCharacter.y + (int)direction.Y].transform.position, turnTime)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() =>
                {
                  //TODO: Show dead animation if dead
                  isFinishAnimation = true;
                });
            }
          }
          else
          {
            characterGOs[currentCharacter.team][currentCharacter.characterRole].transform.position = cellGOs[currentCharacter.x][currentCharacter.y].transform.position;
            isFinishAnimation = true;
          }
        }
      }
    }
    else
    {
      isFinishAnimation = true; //turn 0
    }
    yield return new WaitUntil(() => isFinishAnimation);
    for (int team = 0; team < 2; team++)
    {
      for (int i = 0; i < 3; i++)
      {
        characters = logs[currentTurn].serverGameState.characters;
        var currentCharacter = characters[(Team)team][(CharacterRole)i];
        characterGOs[currentCharacter.team][currentCharacter.characterRole].transform.position = cellGOs[currentCharacter.x][currentCharacter.y].transform.position;
      }
    }
    yield return ChangeMap(currentTurn);
  }

  bool IsValidDirection(int currentX, int currentY, int directionX, int directionY)
  {
    if (currentX + directionX >= cols || currentX + directionX < 0) return false;
    if (currentY + directionY >= rows || currentY + directionY < 0) return false;
    return true;
  }

  void InitGrid()
  {
    GameObject cellTile = new GameObject("Cell");
    GameObject cellObject = new GameObject("CellObject");

    cellObject.AddComponent<SpriteRenderer>();
    cellTile.AddComponent<SpriteRenderer>();

    Vector2 newCellSize = new Vector2(gridSize.x / (float)cols, gridSize.y / (float)rows);

    cellScale.x = newCellSize.x / cellSize.x;
    cellScale.y = newCellSize.y / cellSize.y;
    cellSize = newCellSize;
    cellTile.transform.localScale = new Vector2(cellScale.x, cellScale.y);

    gridOffset.x = -(gridSize.x / 2) + cellSize.x / 2;
    gridOffset.y = -(gridSize.y / 2) + cellSize.y / 2;

    //fill the grid with cells by using Instantiate
    for (int col = 0; col < cols; col++)
    {
      var temp = new List<GameObject>();
      for (int row = rows - 1; row >= 0; row--)
      {
        Vector2 pos = new Vector2(col * cellSize.x + gridOffset.x + transform.position.x, row * cellSize.y + gridOffset.y + transform.position.y);
        GameObject cO = Instantiate(cellObject, pos, Quaternion.identity, gridTransform) as GameObject;
        cO.GetComponent<SpriteRenderer>().sprite = GetSpriteByTileType(mapInfo[col][rows - row - 1].type, mapInfo[col][rows - row - 1].growState);
        cO.GetComponent<SpriteRenderer>().sortingOrder = 1;
        cO.transform.parent = gridTransform;
        temp.Add(cO);

        GameObject backgroundTile = Instantiate(cellTile, pos, Quaternion.identity, gridTransform);
        backgroundTile.GetComponent<SpriteRenderer>().sprite = emptySprite;
        Color color = Color.white;
        if (row % 2 == 0)
        {
          if (col % 2 == 0) ColorUtility.TryParseHtmlString("#45A842", out color);
          else ColorUtility.TryParseHtmlString("#5DBC59", out color);
        }
        else
        {
          if (col % 2 == 0) ColorUtility.TryParseHtmlString("#5DBC59", out color);
          else ColorUtility.TryParseHtmlString("#45A842", out color);
        }
        backgroundTile.GetComponent<SpriteRenderer>().color = color;
      }
      cellGOs.Add(temp);
    }

    Destroy(cellTile);
    Destroy(cellObject);
  }

  void InitCharacter()
  {
    for (int team = 0; team < 2; team++)
    {
      var newDictionary = new Dictionary<CharacterRole, GameObject>();
      for (int i = 0; i < 3; i++)
      {
        var characterInfo = logs[currentTurn].serverGameState.characters[(Team)team][(CharacterRole)i];
        var cell = cellGOs[characterInfo.x][characterInfo.y];
        GameObject characterGO = Instantiate(GetPrefabByRole((Team)team, (CharacterRole)i), cell.transform.position, Quaternion.identity, gridTransform);
        var characterSprite = characterGO.GetComponent<SpriteRenderer>().sprite;
        characterGO.transform.localScale = new Vector2(characterSize.x / characterSprite.bounds.size.x, characterSize.y / characterSprite.bounds.size.y);
        newDictionary.Add((CharacterRole)i, characterGO);
      }
      characterGOs.Add((Team)team, newDictionary);
    }
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
        return null;
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

  GameObject GetPrefabByRole(Team team, CharacterRole role)
  {
    if (team == 0)
      switch (role)
      {
        case CharacterRole.Planter:
          return redPlanterPrefab;
        case CharacterRole.Harvester:
          return redHarvesterPrefab;
        case CharacterRole.Worm:
          return redWormPrefab;
      }
    else
      switch (role)
      {
        case CharacterRole.Planter:
          return bluePlanterPrefab;
        case CharacterRole.Harvester:
          return blueHarvesterPrefab;
        case CharacterRole.Worm:
          return blueWormPrefab;
      }
    return null;
  }

  //---------------------------------------------Play Scene UI Actions --------------------------------------------------
  private void PlayButtonClick()
  {
    StartCoroutine(PlayLogs(true));
  }

  private void StopPlayRecord()
  {
    playRecordState = PlayRecordState.Stop;
    DOTween.CompleteAll();
    StopAllCoroutines();
  }

  private void PrevButtonClick()
  {
    if (currentTurn - 1 >= 0)
    {
      currentTurn--;
      ChangeTurn(currentTurn);
    }
  }

  private void NextButtonClick()
  {
    if (currentTurn + 1 < logs.Count)
    {
      currentTurn++;
      ChangeTurn(currentTurn);
    }
  }

  private void ChangeGameSpeedButtonClick(float gameSpeed)
  {
    timeMultiplier = gameSpeed;
    Time.timeScale = timeMultiplier;
  }

  private void ChangeTurn(int currentTurn)
  {
    StopPlayRecord();
    this.currentTurn = currentTurn;
    uiManager.DisplayTurnInfo(currentTurn, logs[currentTurn].serverGameState.blueScore, logs[currentTurn].serverGameState.redScore);
    if (currentTurn == logs.Count - 1) uiManager.ToggleResult(true);
    else uiManager.ToggleResult(false);
    StartCoroutine(ChangeCharacter(currentTurn));
  }
}

public enum PlayRecordState
{
  Playing,
  Stop
}
