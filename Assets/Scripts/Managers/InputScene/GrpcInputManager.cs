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
using Microsoft.Scripting.Hosting;
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
  private bool[] isBot;
  void Awake()
  {
    isBot = new bool[6];
    for (int i = 0; i < 6; i++)
    {
      isBot[i] = true;
    }
    uiManager = GetComponent<InputSceneUI>();
    uiManager.StartGame = StartRecordGame;
    uiManager.LoadAIFolder = OpenFileBrowser;
  }

  private void OpenFileBrowser(int index)
  {
    string path = FileBrowser.OpenSingleFolder("Choose AI folder");
    LoadFileUsingPath(path, index);
  }

  // Loads a file using a path
  private void LoadFileUsingPath(string path, int index)
  {
    if (!String.IsNullOrEmpty(path))
    {
      var copyPath = GetPathByIndex(index, path);
      EmptyDirectory(copyPath);
      CopyAllDirectory(path, copyPath);
      uiManager.ShowOutputText($"Copied file to path: {copyPath}");
      isBot[index] = false;
    }
    else
    {
      uiManager.ShowOutputText("Invalid path given");
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
        tempPath = $"{Application.streamingAssetsPath}/blue_planter";
        Directory.CreateDirectory(tempPath);
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 1:
        tempPath = $"{Application.streamingAssetsPath}/blue_harvester";
        Directory.CreateDirectory(tempPath);
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 2:
        tempPath = $"{Application.streamingAssetsPath}/blue_worm";
        Directory.CreateDirectory(tempPath);
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 3:
        tempPath = $"{Application.streamingAssetsPath}/red_planter";
        Directory.CreateDirectory(tempPath);
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 4:
        tempPath = $"{Application.streamingAssetsPath}/red_harvester";
        Directory.CreateDirectory(tempPath);
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 5:
        tempPath = $"{Application.streamingAssetsPath}/red_worm";
        Directory.CreateDirectory(tempPath);
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      default:
        throw new System.Exception("Index of out bound!!!");
    }
  }

  //-------------------------------------------- Read Python -------------------------------------------------
  public void InitEngine()
  {
    if (channel == null)
    {
      var ip = $"{serverIP}:{Port}";
      channel = new Channel(ip, ChannelCredentials.Insecure);
    }

    for (int i = 0; i < isBot.Length; i++)
    {
      if (!isBot[i])
      {
        PythonCharacter character = new PythonCharacter(channel);
        character.Character = new Character();
        characters.Add(character);
      }
      else
      {
        BotCharacter character = new BotCharacter($"Bot-{i + 1}");
        character.Character = new Character();
        characters.Add(character);
      }
    }
  }

  async void StartRecordGame()
  {
    InitEngine();

    GameLogic gameLogic = new GameLogic();
    var recordManager = gameObject.AddComponent<RecordManager>();

    await gameLogic.StartGame(
      characters[0],
      characters[1],
      characters[2],
      characters[3],
      characters[4],
      characters[5],
      recordManager
    );

    // uiManager.ShowOutputText("End game!");
    StartCoroutine(uiManager.StartLoadingPlayScene());
  }
}
