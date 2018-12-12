using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecordManager : MonoBehaviour, IReplayRecorder
{
  public IInputSceneUI uiManager;
  private List<RecordModel> log = new List<RecordModel>();
  private string jsonFilePath;

  void Awake()
  {
    uiManager = GetComponent<InputSceneUI>();
    jsonFilePath = $"{Application.streamingAssetsPath}/logs";
  }

  public void LogEndGame(ServerGameState serverGameState)
  {
    var dateString = DateTime.Now;
    var fileName = $"log-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.json";
    if (!Directory.Exists(jsonFilePath))
      Directory.CreateDirectory(jsonFilePath);

    string json = JsonConvert.SerializeObject(log, Formatting.Indented);
    File.WriteAllText($"{jsonFilePath}/{fileName}", json);
    uiManager.SaveErrorMessage($"Recorded into file: {jsonFilePath}/{fileName}");
    PlayerPrefs.SetString("LogPath", $"{jsonFilePath}/{fileName}");
  }


  public void LogGameState(ServerGameState serverGameState)
  {
    RecordModel recordModel = new RecordModel();
    recordModel.serverGameState = JsonConvert.DeserializeObject<ServerGameState>(JsonConvert.SerializeObject(serverGameState));
    recordModel.serverGameState.turn = 0;
    recordModel.actions = null;
    log.Add(recordModel);
  }


  public void LogTurn(ServerGameState serverGameState, List<TurnAction> actions)
  {
    uiManager.SaveErrorMessage($"Recorded turn: {serverGameState.turn}");
    uiManager.ShowRecordingProcess(serverGameState.turn, GameConfigs.GAME_LENGTH);
    RecordModel recordModel = new RecordModel();
    recordModel.serverGameState = JsonConvert.DeserializeObject<ServerGameState>(JsonConvert.SerializeObject(serverGameState));
    recordModel.actions = actions;
    log.Add(recordModel);
  }
}