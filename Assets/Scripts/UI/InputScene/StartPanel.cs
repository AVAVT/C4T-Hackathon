using System;
using UnityEngine;

public class StartPanel: MonoBehaviour
{
  public Action EnableInputPanel { private get; set; }
  public Action EnablePlayRecordPanel { private get; set; }
  public Action EnableSettingPanel { private get; set; }
  public void OnStartGameButtonClick()
  {
    EnableInputPanel?.Invoke();
  }

  public void OnPlayRecordButtonClick()
  {
    EnablePlayRecordPanel?.Invoke();
  }

  public void OnSettingButtonClick()
  {
    EnableSettingPanel?.Invoke();
  }

  public void OnExitButtonClick()
  {
    Application.Quit();
  }
}