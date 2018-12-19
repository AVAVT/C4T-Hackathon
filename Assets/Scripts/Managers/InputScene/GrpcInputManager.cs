using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Crosstales.FB;
using Grpc.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GrpcInputManager : MonoBehaviour
{
  public MapModel mapModel;
  public IInputSceneUI uiManager;
  private ErrorRecorder errorRecorder;
  //characters
  TeamRoleMap<ICharacterController> characters = new TeamRoleMap<ICharacterController>();
  private bool[] isBot;

  //client
  [SerializeField] private string serverIP;
  [SerializeField] private int Port = 50051;
  private Channel channel;
  private Process pythonProcess;
  private CancellationTokenSource tokenSource;
  private CancellationToken cancellationToken;

  //map
  private int currentMap = 0;

  void Awake()
  {
    isBot = new bool[6];
    for (int i = 0; i < 6; i++)
    {
      isBot[i] = true;
    }
    uiManager.ChangeMap = ChangeMap;
    uiManager.SetIsBot = SetIsBot;
    uiManager.StartGame = StartGame;
  }

  private void SetIsBot(int index)
  {
    isBot[index] = false;
  }

  //-------------------------------------------- Read Python -------------------------------------------------
  public TeamRoleMap<ICharacterController> InitCharacter(MapInfo mapInfo, GameConfig gameRule, ErrorRecorder errorRecorder)
  {
    var controllers = new TeamRoleMap<ICharacterController>();
    int botIndex = 0; // TODO change bot to use the same configurable system as controllers
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        if (!isBot[botIndex])
        {
          PythonCharacter character = new PythonCharacter(channel);
          character.CancelStartGameTask = StopRecordGameWhenError;

          controllers.SetItem(team, role, character);
        }
        else
        {
          BotCharacter character = new BotCharacter();
          character.uiManager = this.uiManager;
          character.mapInfo = mapInfo;

          controllers.SetItem(team, role, character);
        }

        botIndex++;
      }
    }

    return controllers;
  }

  private bool isHavingBot()
  {
    for (int i = 0; i < isBot.Length; i++)
    {
      if (isBot[i]) return true;
    }
    return false;
  }

  private bool NeedStartServer()
  {
    for (int i = 0; i < isBot.Length; i++)
    {
      if (!isBot[i]) return true;
    }
    return false;
  }

  private IEnumerator StartServer()
  {
    if (channel == null)
    {
      var ip = $"{serverIP}:{Port}";
      channel = new Channel(ip, ChannelCredentials.Insecure);
    }
    string botNoti = ShowBot();
    if (NeedStartServer())
    {
      var pythonPath = PlayerPrefs.GetString("PythonPath");
      if (!String.IsNullOrEmpty(pythonPath))
      {
        var serverPath = $"{Application.streamingAssetsPath}/ai_server.py";
        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = pythonPath;
        pythonProcess.StartInfo.Arguments = serverPath;
        pythonProcess.StartInfo.RedirectStandardOutput = true;
        pythonProcess.StartInfo.RedirectStandardError = true;
        pythonProcess.StartInfo.UseShellExecute = false;
        pythonProcess.EnableRaisingEvents = true;

        pythonProcess.OutputDataReceived += new DataReceivedEventHandler(OnDataReceived);
        pythonProcess.ErrorDataReceived += new DataReceivedEventHandler(OnErrorReceived);

        while (!pythonProcess.Start())
        {
          yield return null;
        }
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();
      }
      else
      {
        uiManager.ShowNotiPanel($"Following path to python.exe in settings is not valid: {pythonPath}", 2, 1);
        UnityEngine.Debug.LogError($"Path to python.exe in settings is not valid! Path: {pythonPath}");
      }
    }

    if (isHavingBot())
    {
      uiManager.ShowNotiPanel(botNoti, 4, 1);
      yield return new WaitForSeconds(4);
    }

    tokenSource = new CancellationTokenSource();
    cancellationToken = tokenSource.Token;
    StartRecordGame(cancellationToken);
  }

  private void OnDataReceived(object sender, DataReceivedEventArgs e)
  {
    if (e.Data != null)
    {
      errorRecorder.RecordErrorMessage(e.Data, false);
    }
  }
  private void OnErrorReceived(object sender, DataReceivedEventArgs e)
  {
    if (e.Data != null)
    {
      errorRecorder.RecordErrorMessage(e.Data, true);
    }
  }
  private string ShowBot()
  {
    string currentBots = "Following characters are controlled by bot:";
    for (int team = 0; team < 2; team++)
    {
      for (int i = 0; i < 3; i++)
      {
        if (isBot[team * 3 + i])
        {
          currentBots += $"\nTeam: {(Team)team} - Character: {(CharacterRole)i}";
        }
      }
    }
    return currentBots;
  }

  public void StartGame()
  {
    StartCoroutine(StartServer());
  }

  public void StopRecordGameWhenError()
  {
    UnityEngine.Debug.Log("Cancel task!");
    tokenSource.Cancel();
  }

  async void StartRecordGame(CancellationToken token)
  {
    GameConfig gameRule = GameConfig.DefaultGameRule();
    errorRecorder = new ErrorRecorder();

    var mapInfo = mapModel.listMap[currentMap].mapDisplayData.ToMapInfo(gameRule);
    var gameLogic = GameLogic.GameLogicForPlay(gameRule, mapInfo);
    var recordManager = gameObject.AddComponent<RecordManager>();

    // TODO record game rule
    characters = InitCharacter(mapInfo, gameRule, errorRecorder);
    gameLogic.InitializeGame(characters);

    var task = gameLogic.PlayGame(
      cancellationToken,
      recordManager
    );

    await task;

    if (task.IsFaulted)
    {
      errorRecorder.RecordErrorMessage($"Start game fail! Error message: {task.Exception}", true);
      uiManager.ShowRecordPanelWhenError(errorRecorder.ErrorMessage);
    }
    else if (task.IsCanceled)
    {
      errorRecorder.RecordErrorMessage($"Start game fail! Start game task is canceled! Error message: {task.Exception}", true);
      uiManager.ShowRecordPanelWhenError(errorRecorder.ErrorMessage);
    }
    else
    {
      if (!errorRecorder.HaveError)
        StartCoroutine(ShowLogPathNoti());
      else
        uiManager.ShowRecordPanelWhenError(errorRecorder.ErrorMessage);
    }
  }

  IEnumerator ShowLogPathNoti()
  {
    uiManager.ShowNotiPanel($"Log file is saved to following path: {PlayerPrefs.GetString("LogPath")}", 4, 1);
    pythonProcess.Kill();
    pythonProcess.Dispose();
    yield return new WaitForSeconds(4);
    uiManager.StartLoadingPlayScene();
  }

  void ChangeMap(bool isNext)
  {
    if (isNext)
    {
      currentMap++;
      if (currentMap >= mapModel.listMap.Count) currentMap = 0;
    }
    else
    {
      currentMap--;
      if (currentMap < 0) currentMap = mapModel.listMap.Count - 1;
    }
    uiManager.ShowMapInfo(mapModel.listMap[currentMap].mapDisplayData);
  }
}
