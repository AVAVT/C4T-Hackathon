using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecordManager : MonoBehaviour, IReplayRecorder
{
  public IErrorRecorder errorRecorder;
  public IInputSceneUI uiManager;
  private List<RecordModel> log = new List<RecordModel>();
  private string jsonFilePath;
  GameConfig gameConfig;

  public void LogEndGame(ServerGameState serverGameState)
  {
    WriteLogToFile();
  }

  public void LogGameStart(GameConfig gameRule, ServerGameState serverGameState)
  {
    this.gameConfig = gameRule;
    RecordModel recordModel = new RecordModel();
    recordModel.serverGameState = JsonConvert.DeserializeObject<ServerGameState>(JsonConvert.SerializeObject(serverGameState));
    recordModel.serverGameState.turn = 0;
    recordModel.actions = null;
    log.Add(recordModel);
  }

  public void LogTurn(ServerGameState serverGameState, List<TurnAction> actions)
  {
    errorRecorder.RecordErrorMessage($"Recorded turn: {serverGameState.turn}", false);
    uiManager.ShowRecordingProcess(serverGameState.turn, gameConfig.gameLength);
    RecordModel recordModel = new RecordModel();
    recordModel.serverGameState = JsonConvert.DeserializeObject<ServerGameState>(JsonConvert.SerializeObject(serverGameState));
    recordModel.actions = actions;
    log.Add(recordModel);
  }

  void WriteLogToFile()
  {
    jsonFilePath = PlayerPrefs.GetString("SaveLogPath", $"{GrpcInputManager.DATAPATH}/logs");
    var dateString = DateTime.Now;
    var fileName = $"log-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.json";
    if (!Directory.Exists(jsonFilePath))
      Directory.CreateDirectory(jsonFilePath);
    var gameRecord = new GameRecordLogData();
    gameRecord.gameConfig = gameConfig;
    gameRecord.log = log;
    string json = JsonConvert.SerializeObject(gameRecord, Formatting.Indented);
    File.WriteAllText($"{jsonFilePath}/{fileName}", json);
    PlayerPrefs.SetString("LogPath", $"{jsonFilePath}/{fileName}");
  }
}

[System.Serializable]
public class GameRecordLogData
{
  public GameConfig gameConfig;
  public List<RecordModel> log;
}