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
  private string[] paths;
  private ScriptSource[] sources;
  private ScriptScope[] scopes;
  private ScriptEngine engine;

  void Awake()
  {
    paths = new string[6];
    sources = new ScriptSource[6];
    scopes = new ScriptScope[6];
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
      var copyPath = GetPathByIndex(currentAIIndex, path);
      File.Copy(path, copyPath, true);
      UnityEngine.Debug.Log($"Copied file to path: {copyPath}");
      textOutputManager.ShowOutputText($"Copied file to path: {copyPath}");
    }
    else
    {
      textOutputManager.ShowOutputText("Invalid path given");
    }
  }

  private string GetPathByIndex(int index, string path)
  {
    string tempPath;
    switch (index)
    {
      case 0:
        tempPath = $"{Application.streamingAssetsPath}/orc-planter.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 1:
        tempPath = $"{Application.streamingAssetsPath}/orc-harvester.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 2:
        tempPath = $"{Application.streamingAssetsPath}/orc-worm.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 3:
        tempPath = $"{Application.streamingAssetsPath}/human-planter.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 4:
        tempPath = $"{Application.streamingAssetsPath}/human-harvester.py";
        paths[index] = tempPath;
        textOutputManager.ShowPathByIndex(index, path);
        return tempPath;
      case 5:
        tempPath = $"{Application.streamingAssetsPath}/human-worm.py";
        paths[index] = tempPath;
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
      for (int i = 0; i < paths.Length; i++)
      {
        sources[i] = engine.CreateScriptSourceFromFile(paths[i]);
        scopes[i] = engine.CreateScope();
      }
    }
    catch (System.Exception ex)
    {
      UnityEngine.Debug.Log(ex);
      return;
    }
  }

  public void WaitAIResponse(int i, string className, string methodName)
  {
    //start new task
    var ct = new CancellationTokenSource(500);
    var tcs = new TaskCompletionSource<bool>();
    ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
    var myClass = GetObject(i, className);
    Stopwatch sw = Stopwatch.StartNew();
    Task<string>.Factory.StartNew(() => GetAIResponse(myClass, methodName, new object[] { "Hi" }), ct.Token).ContinueWith((task) =>
    {
      sw.Stop();
      UnityEngine.Debug.Log(sw.ElapsedMilliseconds);

      if (task.IsFaulted)
      {
        throw task.Exception;
      }
      else if (task.IsCanceled || ct.IsCancellationRequested)
      {
        UnityEngine.Debug.Log("Time out!");
        //TODO: Time out action -> lose turn
      }
      else
      {
        ct.Cancel();
        UnityEngine.Debug.Log("Task completed!!!");
        UnityEngine.Debug.Log($"Result: {task.Result}");
      }
    });
  }

  System.Object GetObject(int i, string className)
  {
    sources[i].Execute(scopes[i]);
    return engine.Operations.Invoke(scopes[i].GetVariable(className));
  }

  private string GetAIResponse(System.Object myclass, string methodName, object[] parameters)
  {
    try
    {
      return engine.Operations.InvokeMember(myclass, methodName, parameters);
    }
    catch (System.Exception ex)
    {
      UnityEngine.Debug.Log(ex);
    }
    return null;
  }

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
      UnityEngine.Debug.Log(result);
      return result;
    }
    catch (System.Exception ex)
    {
      var foo = ex.Message;
      UnityEngine.Debug.Log($"Error: {ex.Message}\n");
      return ex.Message;
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
}
