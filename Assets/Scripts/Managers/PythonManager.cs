using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GracesGames.SimpleFileBrowser.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PythonManager : MonoBehaviour, IPythonInterpreter
{
  public ITextOutput textOutputManager;

  [Header("File loader")]
  public GameObject FileBrowserPrefab;
  public string[] FileExtensions;
  public bool PortraitMode;
  private int currentAIIndex;
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
      var copyPath = GetPathByIndex(currentAIIndex);
      File.Copy(path, copyPath, true);
      Debug.Log($"Copied file to path: {copyPath}");
      textOutputManager.ShowOutputText($"Copied file to path: {copyPath}");
    }
    else
    {
      textOutputManager.ShowOutputText("Invalid path given");
    }
  }

  private string GetPathByIndex(int index)
  {
    string tempPath;
    switch (index)
    {
      case 0:
        tempPath = $"{Application.streamingAssetsPath}/orc-farmer.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, tempPath);
        return tempPath;
      case 1:
        tempPath = $"{Application.streamingAssetsPath}/orc-collector.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, tempPath);
        return tempPath;
      case 2:
        tempPath = $"{Application.streamingAssetsPath}/orc-worm.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, tempPath);
        return tempPath;
      case 3:
        tempPath = $"{Application.streamingAssetsPath}/human-farmer.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, tempPath);
        return tempPath;
      case 4:
        tempPath = $"{Application.streamingAssetsPath}/human-collector.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, tempPath);
        return tempPath;
      case 5:
        tempPath = $"{Application.streamingAssetsPath}/human-worm.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, tempPath);
        return tempPath;
      default:
        throw new System.Exception("Index of out bound!!!");
    }
  }

  //-------------------------------------------- Read Python -------------------------------------------------
  public string GetResult(string fileName, string className, string methodName)
  {
    try
    {
      var engine = UnityPython.CreateEngine();
      var path = $"{Application.streamingAssetsPath}/{fileName}.py";
      var source = engine.CreateScriptSourceFromFile(path);
      var scope = engine.CreateScope();
      source.Execute(scope);

      System.Object myclass = engine.Operations.Invoke(scope.GetVariable(className));
      object[] parameters = new object[] { "Hi" };
      var result = engine.Operations.InvokeMember(myclass, methodName, parameters);
      Debug.Log(result);
      return result;
    }
    catch (System.Exception ex)
    {
      var foo = ex.Message;
      Debug.Log($"Error: {ex.Message}\n");
      return ex.Message;
    }
  }
}
