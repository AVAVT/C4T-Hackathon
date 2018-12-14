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

  //characters
  List<ICharacterController> characters = new List<ICharacterController>();
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
    uiManager = GetComponent<InputSceneUI>();
    uiManager.StartGame = StartGame;
    uiManager.LoadAIFolder = OpenFileBrowser;
    uiManager.ChangeMap = ChangeMap;
  }

  private bool isFolderContainPython(string path)
  {
    foreach (var dir in Directory.GetFiles(path))
    {
      if (dir.Contains("main.py")) return true;
    }
    return false;
  }

  public void OpenFileBrowser(int index)
  {
    string path = FileBrowser.OpenSingleFolder("Choose AI folder");
    if (!String.IsNullOrEmpty(path) && isFolderContainPython(path))
    {
      var copyPath = GetPathByIndex(index, path);
      EmptyDirectory(copyPath);
      CopyAllDirectory(path, copyPath);
      uiManager.SaveErrorMessage($"Copied file to path: {copyPath}", false);
      uiManager.ShowNotiPanel($"Import folder containing main.py successfully!", 2, 1);
      uiManager.ShowFileStatus(index);
      isBot[index] = false;
    }
    else
    {
      uiManager.ShowNotiPanel("Invalid path given or folder does not contain main.py file!", 2, 1);
    }
  }

  private void CopyAllDirectory(string sourceDir, string targetDir)
  {
    foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
      Directory.CreateDirectory(Path.Combine(targetDir, dir.Substring(sourceDir.Length + 1)));
    foreach (var fileName in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
      File.Copy(fileName, Path.Combine(targetDir, fileName.Substring(sourceDir.Length + 1)), true);
  }

  private void EmptyDirectory(string path)
  {
    if (Directory.Exists(path))
    {
      Directory.Delete(path, true);
    }
    Directory.CreateDirectory(path);
  }

  private string GetPathByIndex(int index, string path)
  {
    string tempPath;
    switch (index)
    {
      case 0:
        tempPath = $"{Application.streamingAssetsPath}/red_planter";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 1:
        tempPath = $"{Application.streamingAssetsPath}/red_harvester";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 2:
        tempPath = $"{Application.streamingAssetsPath}/red_worm";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 3:
        tempPath = $"{Application.streamingAssetsPath}/blue_planter";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 4:
        tempPath = $"{Application.streamingAssetsPath}/blue_harvester";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 5:
        tempPath = $"{Application.streamingAssetsPath}/blue_worm";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      default:
        throw new System.Exception("Index of out bound!!!");
    }
  }

  //-------------------------------------------- Read Python -------------------------------------------------
  public void InitCharacter(Map mapInfo)
  {
    for (int i = 0; i < isBot.Length; i++)
    {
      if (!isBot[i])
      {
        PythonCharacter character = new PythonCharacter(channel);
        character.Character = new Character();
        characters.Add(character);
        character.uiManager = this.uiManager;
        character.CancelStartGameTask = StopRecordGameWhenError;
      }
      else
      {
        BotCharacter character = new BotCharacter($"Bot-{i + 1}");
        character.Character = new Character();
        character.uiManager = this.uiManager;
        character.mapInfo = mapInfo;
        characters.Add(character);
      }
    }
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

  private void InitMapInfo(Map mapInfo)
  {
    mapInfo.RED_BOX_POS = new System.Numerics.Vector2(0, 0);
    mapInfo.BLUE_BOX_POS = new System.Numerics.Vector2(mapInfo.MAP_WIDTH - 1, mapInfo.MAP_HEIGHT - 1);
    mapInfo.RED_ROCK_POS = new System.Numerics.Vector2(0, mapInfo.MAP_HEIGHT - 1);
    mapInfo.BLUE_ROCK_POS = new System.Numerics.Vector2(mapInfo.MAP_WIDTH - 1, 0);

    mapInfo.RED_STARTING_POSES = new List<System.Numerics.Vector2>()
      {
        mapInfo.RED_BOX_POS,
        mapInfo.RED_BOX_POS,
        mapInfo.RED_ROCK_POS
      };
    mapInfo.BLUE_STARTING_POSES = new List<System.Numerics.Vector2>()
      {
        mapInfo.BLUE_BOX_POS,
        mapInfo.BLUE_BOX_POS,
        mapInfo.BLUE_ROCK_POS
      };

    mapInfo.WATER_POSES = new List<System.Numerics.Vector2>();
    foreach (var waterPos in mapInfo.WATER_POSES_UNITY)
    {
      mapInfo.WATER_POSES.Add(new System.Numerics.Vector2(waterPos.x, waterPos.y));
    }

    mapInfo.WILDBERRY_POSES = new List<System.Numerics.Vector2>();
    foreach (var berryPos in mapInfo.WILDBERRY_POSES_UNITY)
    {
      mapInfo.WILDBERRY_POSES.Add(new System.Numerics.Vector2(berryPos.x, berryPos.y));
    }
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
      uiManager.SaveErrorMessage(e.Data, false);
    }
  }
  private void OnErrorReceived(object sender, DataReceivedEventArgs e)
  {
    if (e.Data != null)
    {
      uiManager.SaveErrorMessage(e.Data, true);
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
    // tokenSource.Token.ThrowIfCancellationRequested();
  }

  async void StartRecordGame(CancellationToken token)
  {
    var mapInfo = mapModel.listMap[currentMap].mapConfig;
    InitMapInfo(mapInfo);

    var gameLogic = new GameLogic(mapInfo);
    var recordManager = gameObject.AddComponent<RecordManager>();
    InitCharacter(mapInfo);

    var task = gameLogic.StartGame(
      characters[0],
      characters[1],
      characters[2],
      characters[3],
      characters[4],
      characters[5],
      cancellationToken,
      recordManager
    );

    await task;
    if (task.IsFaulted)
    {
      uiManager.SaveErrorMessage($"Start game fail! Error message: {task.Exception}", true);
      uiManager.ShowRecordPanelWhenError();
    }
    else if (task.IsCanceled)
    {
      uiManager.SaveErrorMessage($"Start game fail! Start game task is canceled! Error message: {task.Exception}", true);
      uiManager.ShowRecordPanelWhenError();
    }
    else
    {
      if (!uiManager.HaveError)
        StartCoroutine(ShowLogPathNoti());
      else
        uiManager.ShowRecordPanelWhenError();
    }
  }

  IEnumerator ShowLogPathNoti()
  {
    uiManager.ShowNotiPanel($"Log file is saved to following path: {PlayerPrefs.GetString("LogPath")}", 4, 1);
    yield return new WaitForSeconds(4);
    yield return uiManager.StartLoadingPlayScene();
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
    uiManager.ShowMapInfo(mapModel.listMap[currentMap].mapConfig);
  }
}
