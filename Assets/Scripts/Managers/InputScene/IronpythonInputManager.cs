using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Crosstales.FB;
using Microsoft.Scripting.Hosting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class IronpythonInputManager : MonoBehaviour
{
  public IInputSceneUI uiManager;

  //Iron Python
  private ScriptEngine engine;
  List<ICharacterController> characters = new List<ICharacterController>();
  private string[] paths;

  void Awake()
  {
    uiManager = GetComponent<InputSceneUI>();
    paths = new string[6];
    uiManager.StartGame = StartRecordGame;
  }

  public void OpenFileBrowser(int index)
  {
    paths[index] = FileBrowser.OpenSingleFolder("Choose AI folder");
    LoadFileUsingPath(paths[index], index);
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
        paths[index] = $"{tempPath}/main.py";
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 1:
        tempPath = $"{Application.streamingAssetsPath}/blue_harvester";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 2:
        tempPath = $"{Application.streamingAssetsPath}/blue_worm";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 3:
        tempPath = $"{Application.streamingAssetsPath}/red_planter";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 4:
        tempPath = $"{Application.streamingAssetsPath}/red_harvester";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        uiManager.ShowPathByIndex(index, path);
        return tempPath;
      case 5:
        tempPath = $"{Application.streamingAssetsPath}/red_worm";
        Directory.CreateDirectory(tempPath);
        paths[index] = $"{tempPath}/main.py";
        uiManager.ShowPathByIndex(index, path);
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
        if (paths[i] != null && !String.IsNullOrEmpty(paths[i]))
        {
          var source = engine.CreateScriptSourceFromFile(paths[i]);
          var scope = engine.CreateScope();
          IronpythonCharacter character = new IronpythonCharacter(engine, source, scope);
          character.Character = new Character();
          characters.Add(character);

          if (!String.IsNullOrEmpty(Path.GetDirectoryName(paths[i])))
            searchPaths.Add(Path.GetDirectoryName(paths[i]));
          else
            searchPaths.Add(Environment.CurrentDirectory);
            //for required libs
          searchPaths.Add(Application.streamingAssetsPath);
        }
        else
        {
          BotCharacter character = new BotCharacter($"Bot-{i}");
          character.Character = new Character();
          characters.Add(character);
        }
      }
      engine.SetSearchPaths(searchPaths);
    }
    catch (System.Exception ex)
    {
      uiManager.ShowOutputText(ex.ToString());
      return;
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
