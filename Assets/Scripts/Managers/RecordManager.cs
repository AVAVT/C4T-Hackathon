using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class RecordManager : IReplayRecorder
{
  private List<RecordModel> log = new List<RecordModel>();
  private string jsonFilePath = $"{Application.streamingAssetsPath}/logs";
  public void LogEndGame(ServerGameState serverGameState)
  {
    Debug.Log("Log End game!!!");
    var dateString = DateTime.Now;
    var fileName = $"log-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.json";
    if (!Directory.Exists(jsonFilePath))
      Directory.CreateDirectory(jsonFilePath);

    string json = JsonConvert.SerializeObject(new LogFile(log), Formatting.Indented);
    File.WriteAllText($"{jsonFilePath}/{fileName}", json);
    Debug.Log($"Recorded into file: {jsonFilePath}/{fileName}");
  }

  public void LogGameState(ServerGameState serverGameState)
  {
    Debug.Log("Log Start game!!!");
    RecordModel recordModel = new RecordModel();
    recordModel.serverGameState= JsonConvert.DeserializeObject<ServerGameState>(JsonConvert.SerializeObject(serverGameState));
    recordModel.actions = null;
    log.Add(recordModel);
  }


  public void LogTurn(ServerGameState serverGameState, List<TurnAction> actions)
  {
    Debug.Log($"Recorded turn: {serverGameState.turn}");
    RecordModel recordModel = new RecordModel();
    recordModel.serverGameState= JsonConvert.DeserializeObject<ServerGameState>(JsonConvert.SerializeObject(serverGameState));
    recordModel.actions = actions;
    log.Add(recordModel);
  }
}

public class LogFile
{
  public List<RecordModel> log;
  public LogFile(List<RecordModel> log)
  {
    this.log = log;
  }
}