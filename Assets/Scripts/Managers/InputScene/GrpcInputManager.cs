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
  public IInputSceneUI uiManager;

  List<ICharacterController> characters = new List<ICharacterController>();
  [SerializeField] private string serverIP;
  [SerializeField] private int Port = 50051;
  private Channel channel;
  private Process pythonProcess;
  private bool[] isBot;
  private bool needStartServer = false;
  private int botNum = 0;
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
      needStartServer = true;
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
  public void InitCharacter()
  {
    for (int i = 0; i < isBot.Length; i++)
    {
      if (!isBot[i])
      {
        PythonCharacter character = new PythonCharacter(channel);
        character.Character = new Character();
        characters.Add(character);
        character.uiManager = this.uiManager;
      }
      else
      {
        BotCharacter character = new BotCharacter($"Bot-{i + 1}");
        character.Character = new Character();
        character.uiManager = this.uiManager;
        characters.Add(character);
      }
    }
  }

  private IEnumerator StartServer()
  {
    if (channel == null)
    {
      var ip = $"{serverIP}:{Port}";
      channel = new Channel(ip, ChannelCredentials.Insecure);
    }

    if (needStartServer)
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
        pythonProcess.StartInfo.WorkingDirectory = Application.streamingAssetsPath;
        pythonProcess.EnableRaisingEvents = true;
        // pythonProcess.OutputDataReceived += new DataReceivedEventHandler(OnDataReceived);
        // pythonProcess.ErrorDataReceived += new DataReceivedEventHandler(OnDataReceived);

        while (!pythonProcess.Start())
        {
          yield return null;
        }
      }
      else
      {
        uiManager.ShowNotiPanel($"Following path to python.exe in settings is not valid: {pythonPath}", 2, 1);
        UnityEngine.Debug.LogError($"Path to python.exe in settings is not valid! Path: {pythonPath}");
      }
    }

    string botNoti = ShowBot();
    UnityEngine.Debug.Log(botNum);
    if (botNum != 0)
    {
      uiManager.ShowNotiPanel(botNoti, 4, 1);
      yield return new WaitForSeconds(4);
    }
    StartRecordGame();
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
          botNum++;
        }
      }
    }
    return currentBots;
  }

  public void StartGame()
  {
    StartCoroutine(StartServer());
  }

  async void StartRecordGame()
  {
    InitCharacter();
    GameLogic gameLogic = new GameLogic();
    var recordManager = gameObject.AddComponent<RecordManager>();

    var task = gameLogic.StartGame(
      characters[0],
      characters[1],
      characters[2],
      characters[3],
      characters[4],
      characters[5],
      recordManager
    );
    await task;
    if (task.IsFaulted)
    {
      uiManager.SaveErrorMessage($"Start game fail! Error message: {task.Exception}",true);
      uiManager.ShowRecordPanelWhenError();
    }
    else if (task.IsCanceled)
    {
      uiManager.SaveErrorMessage($"Start game fail! Start game task is canceled! Error message: {task.Exception}", true);
      uiManager.ShowRecordPanelWhenError();
    }
    else
    {
      UnityEngine.Debug.Log(uiManager.HaveError);
      if (!uiManager.HaveError)
        StartCoroutine(ShowLogPathNoti());
      else
        uiManager.ShowRecordPanelWhenError();
    }
  }

  IEnumerator ShowLogPathNoti()
  {
    uiManager.ShowNotiPanel($"Log file is saved to following path: {PlayerPrefs.GetString("LogPath", Application.streamingAssetsPath + "logs/")}", 4, 1);
    yield return new WaitForSeconds(4);
    yield return uiManager.StartLoadingPlayScene();
  }
}
