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
  [Header("File loader")]
  public GameObject FileBrowserPrefab;
  public string[] FileExtensions;
  public bool PortraitMode;
  private string fileName;

  public Text logText;

  public void OpenFileBrowser(string fileName)
  {
    this.fileName = fileName;
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
      File.Copy(path, $"{Application.streamingAssetsPath}/{fileName}.py", true);
      logText.text += $"Copied file to path: {Path.Combine(Application.streamingAssetsPath, fileName)}\n";
    }
    else
    {
      logText.text += "Invalid path given\n";
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
      logText.text += $"Error: {ex.Message}\n";
      return ex.Message;
    }
  }

  public void ReadPythonFileButtonClick()
  {
    string result = GetResult(fileName, "pythonScriptClass", "getAction");
    if(File.Exists($"{Application.streamingAssetsPath}/{fileName}.py"))
      logText.text += $"Result: {result}\n";
    else
      logText.text +="Please load a .py file!\n";
  }
}
