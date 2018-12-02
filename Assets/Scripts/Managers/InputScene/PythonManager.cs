using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using GracesGames.SimpleFileBrowser.Scripts;
using Microsoft.Scripting.Hosting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PythonManager : MonoBehaviour, IPythonInterpreter
{
  public IOutputUI textOutputManager;

  [Header("File loader")]
  public GameObject FileBrowserPrefab;
  public string[] FileExtensions;
  public bool PortraitMode;
  private int currentAIIndex;

  //Iron Python
  private ScriptEngine engine;
  List<ICharacterController> characters = new List<ICharacterController>();
  private string[] paths;

  void Awake()
  {
    paths = new string[6];
  }

  public void OpenFileBrowser(int index)
  {
    currentAIIndex = index;
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
      var sourcePath = Path.GetDirectoryName(path);
      var copyPath = GetPathByIndex(currentAIIndex, path);
      EmptyDirectory(copyPath);
      CopyAllDirectory(sourcePath, copyPath);
      // File.Copy(path, copyPath, true);
      UnityEngine.Debug.Log($"Copied file to path: {copyPath}");
      textOutputManager.ShowOutputText($"Copied file to path: {copyPath}");
    }
    else
    {
      textOutputManager.ShowOutputText("Invalid path given");
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
  }

  private string GetPathByIndex(int index, string path)
  {
    string tempPath;
    switch (index)
    {
      case 0:
        tempPath = $"{Application.streamingAssetsPath}/blue-planter";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 1:
        tempPath = $"{Application.streamingAssetsPath}/blue-harvester";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 2:
        tempPath = $"{Application.streamingAssetsPath}/blue-worm";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 3:
        tempPath = $"{Application.streamingAssetsPath}/red-planter";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 4:
        tempPath = $"{Application.streamingAssetsPath}/red-harvester";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 5:
        tempPath = $"{Application.streamingAssetsPath}/red-worm";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      default:
        throw new System.Exception("Index of out bound!!!");
    }
  }

  //-------------------------------------------- Read Python -------------------------------------------------
  public void InitEngine()
  {
    try
    {
      engine = UnityPython.CreateEngine();
      var searchPaths = engine.GetSearchPaths();
      for (int i = 0; i < paths.Length; i++)
      {
        var source = engine.CreateScriptSourceFromFile(paths[i]);
        var scope = engine.CreateScope();
        PythonAICharacter character = new PythonAICharacter(engine, source, scope);
        character.Character = new Character();
        characters.Add(character);

        if (!String.IsNullOrEmpty(Path.GetDirectoryName(paths[i])))
          searchPaths.Add(Path.GetDirectoryName(paths[i]));
        else
          searchPaths.Add(Environment.CurrentDirectory);
      }
      engine.SetSearchPaths(searchPaths);
    }
    catch (System.Exception ex)
    {
      UnityEngine.Debug.Log(ex);
      return;
    }
  }

  void InitBotCharacter()
  {
    for (int i = 0; i < 6; i++)
    {
      BotCharacter character = new BotCharacter($"Bot-{i}");
      character.Character = new Character();
      characters.Add(character);
    }
  }

  public bool IsHavingAllFiles()
  {
    bool isFull = true;
    for (int i = 0; i < paths.Length; i++)
    {
      if (paths[i] == null || paths[i] == "")
      {
        isFull = false;
        break;
      }
    }
    return isFull;
  }

  public void StartRecordGame()
  {
    // InitEngine();
    InitBotCharacter();

    GameLogic gameLogic = new GameLogic();
    RecordManager recordManager = new RecordManager();

    gameLogic.StartGame(
      characters[0],
      characters[1],
      characters[2],
      characters[3],
      characters[4],
      characters[5],
      recordManager
    ).ContinueWith((task) =>
    {
      if (task.IsFaulted) UnityEngine.Debug.Log($"Task is faulted! Error: {task.Exception}!");
      if (task.IsCanceled) UnityEngine.Debug.Log($"Task is canceled!");
      else UnityEngine.Debug.Log("End game!");
    });
  }
}
