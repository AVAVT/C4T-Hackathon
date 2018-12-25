using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Crosstales.FB;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetupSceneManager : MonoBehaviour
{
  [SerializeField] private InputField pythonPathText;
  [SerializeField] private InputField rootFolderPathText;
  [SerializeField] private Button btnSave;
  [SerializeField] private TMPro.TMP_Text errorText;
  [SerializeField] private LoadingPanel loadingPanel;

  void Start()
  {
#if UNITY_EDITOR
    PlayerPrefs.DeleteAll();
#endif

    btnSave.interactable = false;

    if (PlayerPrefs.GetInt("HaveConfig", 0) == 0)
    {
      var pythonPath = GetPythonPathFromEnvironment();
      if (pythonPath != "")
      {
        PlayerPrefs.SetString("PythonPath", $"{pythonPath}python.exe");
        pythonPathText.text = PlayerPrefs.GetString("PythonPath");
      }
    }
    else
    {
      StartCoroutine(loadingPanel.StartLoadingScene("InputScene"));
    }
  }

  void ValidateFields()
  {
    if (!String.IsNullOrEmpty(pythonPathText.text) && !String.IsNullOrEmpty(rootFolderPathText.text))
    {
      btnSave.interactable = true;
    }
  }

  string GetPythonPathFromEnvironment()
  {
    ProcessStartInfo startInfo = new ProcessStartInfo();
    var pathString = startInfo.EnvironmentVariables["Path"];
    var pathStringSplit = pathString.Split(';');
    foreach (var pathAfterSplit in pathStringSplit)
    {
      if (pathAfterSplit.Contains("Python") && pathAfterSplit.Contains("3"))
      {
        var tempPathSplit = Path.GetDirectoryName(pathAfterSplit).Split('\\');
        if (tempPathSplit[tempPathSplit.Length - 1].Contains("Python")) return pathAfterSplit;
      }
    }
    return "";
  }
  

  private void ChoosePythonBrowser()
  {
    var path = FileBrowser.OpenSingleFile("Choose python.exe in your computer!", "C:/", "exe");
    if (!String.IsNullOrEmpty(path) && path.Contains("python.exe"))
    {
      pythonPathText.text = path;
      PlayerPrefs.SetString("PythonPath", path);
      errorText.text = "Setup path to file python.exe successfully!";
      errorText.gameObject.SetActive(true);
      ValidateFields();
    }
    else
    {
      errorText.text = "Canceled or it is not a path to python.exe!";
      errorText.gameObject.SetActive(true);
    }
  }

  private void ChooseRootFolder()
  {
    var path = FileBrowser.OpenSingleFolder("Choose root directory for python data!", $"{Application.persistentDataPath}");
    if (!String.IsNullOrEmpty(path))
    {
      rootFolderPathText.text = path;
      PlayerPrefs.SetString("RootFolderPath", path);
      errorText.text = "Setup root folder successfully!";
      errorText.gameObject.SetActive(true);
      if(!Directory.Exists($"{path}/logs"))
        Directory.CreateDirectory($"{path}/logs");
      PlayerPrefs.SetString("SaveLogPath", $"{path}/logs");

      ValidateFields();
    }
    else
    {
      errorText.text = "Invalid path given!";
      errorText.gameObject.SetActive(true);
      UnityEngine.Debug.Log("Invalid path given");
    }
  }

  public void OnChoosePythonPathClick()
  {
    ChoosePythonBrowser();
  }

  public void OnChooseRootFolderClick()
  {
    ChooseRootFolder();
  }

  public void OnButtonSaveClick()
  {


    PlayerPrefs.SetInt("HaveConfig", 1);
    StartCoroutine(loadingPanel.StartLoadingScene("InputScene"));
  }
}
