using System;
using System.Collections;
using System.IO;
using Crosstales.FB;
using UnityEngine;

public class PlayRecordPanel : MonoBehaviour
{
  public Action StartLoadingPlayScene { private get; set; }
  public Action<string, float, float> ShowNotiPanel{private get; set;}
  public Action EnableStartPanel {private get; set; }
  private void ChooseRecordBrowser()
  {
    var path = FileBrowser.OpenSingleFile("Choose log file to play!", GrpcInputManager.DATAPATH + "/logs", "json");
    if (!String.IsNullOrEmpty(path))
    {
      PlayerPrefs.SetString("LogPath", path);
      StartLoadingPlayScene?.Invoke();
    }
    else
    {
      UnityEngine.Debug.Log("Invalid path given or folder does not contain main.py file!");
    }
  }

  public void OnChooseRecordButtonClick()
  {
    ChooseRecordBrowser();
  }

  public void OnLastRecordButtonClick()
  {
    Debug.Log(PlayerPrefs.GetString("LogPath"));
    if (File.Exists(PlayerPrefs.GetString("LogPath")))
    {
      StartLoadingPlayScene?.Invoke();
    }
    else
      ShowNotiPanel?.Invoke("Have not recorded any log file or the file has been deleted!", 2, 1);
  }

  public void BackFromPlayRecordPanel()
  {
    EnableStartPanel?.Invoke();
  }
}