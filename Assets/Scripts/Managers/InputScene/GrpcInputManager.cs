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
  public static string DATAPATH { get; private set; }
  public static string PROTOTYPEDATAPATH { get; private set; }
  public MapModel mapModel;
  public IInputSceneUI uiManager;
  private ErrorRecorder errorRecorder;
  //characters
  TeamRoleMap<ICharacterController> characters = new TeamRoleMap<ICharacterController>();
  TeamRoleMap<bool> isBot = new TeamRoleMap<bool>();

  //client
  [SerializeField] private string serverIP;
  [SerializeField] private int Port = 50051;
  private Channel channel;
  private Process pythonProcess;
  private CancellationTokenSource tokenSource;
  private CancellationToken cancellationToken;
  private GameConfig gameRule;

  //map
  private int currentMap = 0;

  void Start()
  {
    DATAPATH = PlayerPrefs.GetString("RootFolderPath");
    PROTOTYPEDATAPATH = $"{Application.streamingAssetsPath}";

    gameRule = GameConfig.DefaultGameRule();
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        isBot.SetItem(team, role, true);
      }
    }

    uiManager.ChangeMap = ChangeMap;
    uiManager.SetIsBot = SetIsBot;
    uiManager.StartGame = StartGame;
  }

  private void CopyAllDirectory(string sourceDir, string targetDir)
  {
    foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
      Directory.CreateDirectory(Path.Combine(targetDir, dir.Substring(sourceDir.Length + 1)));
    foreach (var fileName in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
      File.Copy(fileName, Path.Combine(targetDir, fileName.Substring(sourceDir.Length + 1)), true);
  }

  private void SetIsBot(int index, bool isTrue)
  {
    int team = (int)(index / gameRule.availableRoles.Count);
    int role = index % gameRule.availableRoles.Count;
    isBot.SetItem((Team)team, (CharacterRole)role, isTrue);
  }

  //-------------------------------------------- Read Python -------------------------------------------------
  public TeamRoleMap<ICharacterController> InitCharacter(MapInfo mapInfo, GameConfig gameRule, ErrorRecorder errorRecorder)
  {
    var controllers = new TeamRoleMap<ICharacterController>();
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        if (!isBot.GetItem(team, role))
        {
          PythonCharacter character = new PythonCharacter(channel);
          character.CancelStartGameTask = StopRecordGameWhenError;
          character.errorRecorder = errorRecorder;
          controllers.SetItem(team, role, character);
        }
        else
        {
          BotCharacter character = new BotCharacter(role);
          character.uiManager = this.uiManager;
          character.mapInfo = mapInfo;
          controllers.SetItem(team, role, character);
        }
      }
    }

    return controllers;
  }

  private bool isHavingBot()
  {
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        if (isBot.GetItem(team, role)) return true;
      }
    }
    return false;
  }

  private bool NeedStartServer()
  {
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        if (!isBot.GetItem(team, role)) return true;
      }
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
      CopyAllDirectory(PROTOTYPEDATAPATH, DATAPATH);
      var pythonPath = PlayerPrefs.GetString("PythonPath");
      UnityEngine.Debug.Log(pythonPath);

      if (!String.IsNullOrEmpty(pythonPath))
      {
        var serverPath = $"{DATAPATH}/ai_server.py";
        pythonProcess = new Process();

        // pythonProcess.StartInfo.CreateNoWindow= true;
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
    foreach (var team in gameRule.availableTeams)
    {
      foreach (var role in gameRule.availableRoles)
      {
        if (isBot.GetItem(team, role))
        {
          currentBots += $"\nTeam: {team} - Character: {role}";
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
    errorRecorder = new ErrorRecorder();

    var mapInfo = mapModel.listMap[currentMap].mapDisplayData.ToMapInfo(gameRule);
    var gameLogic = GameLogic.GameLogicForPlay(gameRule, mapInfo);
    var recordManager = gameObject.AddComponent<RecordManager>();
    recordManager.errorRecorder = this.errorRecorder;
    recordManager.uiManager = this.uiManager;

    characters = InitCharacter(mapInfo, gameRule, errorRecorder);
    gameLogic.InitializeGame(characters);

    var task = gameLogic.PlayGame(
      cancellationToken,
      recordManager
    );

    await task;

    KillProcess();
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

  private void KillProcess()
  {
    try
    {
      pythonProcess.Kill();
      pythonProcess.Dispose();
    }
    catch { }
  }
  IEnumerator ShowLogPathNoti()
  {
    uiManager.ShowNotiPanel($"Log file is saved to following path: {PlayerPrefs.GetString("LogPath")}", 4, 1);
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
