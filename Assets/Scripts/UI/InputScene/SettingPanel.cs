using System;
using System.Diagnostics;
using System.IO;
using Crosstales.FB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
  public Action EnableStartPanel{private get; set;}
  public Action EnableSettingPanel{private get; set;}

  [SerializeField] private InputField pythonPathText;
  [SerializeField] private InputField saveLogPathText;
  [SerializeField] private TMP_Text errorText;
  [SerializeField] private GameObject btnSettingBack;

  void Awake()
  {
#if UNITY_EDITOR
    PlayerPrefs.DeleteKey("HaveConfig");
#endif

    if (PlayerPrefs.GetInt("HaveConfig", 0) == 0)
    {
      PlayerPrefs.SetString("SaveLogPath", $"{Application.streamingAssetsPath}/logs");
      saveLogPathText.text = PlayerPrefs.GetString("SaveLogPath");
      var pythonPath = GetPythonPathFromEnvironment();
      if (pythonPath != "")
      {
        PlayerPrefs.SetString("PythonPath", $"{pythonPath}python.exe");
        pythonPathText.text = PlayerPrefs.GetString("PythonPath");
      }
      else
      {
        PlayerPrefs.SetInt("HaveConfig", 1);
        EnableSettingPanel?.Invoke();
        btnSettingBack.SetActive(false);
      }
    }
    else
    {
      pythonPathText.text = PlayerPrefs.GetString("PythonPath");
      saveLogPathText.text = PlayerPrefs.GetString("SaveLogPath");
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
      PlayerPrefs.SetInt("HaveConfig", 1);
      errorText.text = "Setup path to file python.exe successfully!";
      errorText.gameObject.SetActive(true);
      btnSettingBack.SetActive(true);
    }
    else
    {
      errorText.text = "Canceled or it is not a path to python.exe!";
      errorText.gameObject.SetActive(true);
    }
  }

  private void ChooseLogPathBrowser()
  {
    var path = FileBrowser.OpenSingleFolder("Choose directory to save your log files!", $"{Application.streamingAssetsPath}/logs");
    if (!String.IsNullOrEmpty(path))
    {
      saveLogPathText.text = path;
      PlayerPrefs.SetString("SaveLogPath", path);
      PlayerPrefs.SetInt("HaveConfig", 1);
      errorText.text = "Setup save log successfully!";
      errorText.gameObject.SetActive(true);
    }
    else
    {
      errorText.text = "Invalid path given!";
      errorText.gameObject.SetActive(true);
      UnityEngine.Debug.Log("Invalid path given");
    }
  }

  public void ResetSettingPanel()
  {
    errorText.text = "";
    errorText.gameObject.SetActive(false);
    btnSettingBack.gameObject.SetActive(true);
  }

  public void BackFromSettingPanel()
  {
    EnableStartPanel?.Invoke();
  }

  public void OnBrowsePythonPathButtonClick()
  {
    ChoosePythonBrowser();
  }

  public void OnBrowseSaveLogPathButtonClick()
  {
    ChooseLogPathBrowser();
  }
}