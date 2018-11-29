using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour
{
  public IPythonInterpreter pythonManager;
  public IOutputUI uiManager;

  //turn base
  private int turn;

  public void ButtonPlayClick()
  {
    if (pythonManager.IsHavingAllFiles())
    {
      StartGame();
    }
    else
    {
      Debug.Log("Please provide all required AI file for all characters!");
    }
  }

  void StartGame()
  {
    //start engine
    uiManager.SavePlayerName();
    StartCoroutine(pythonManager.StartRecordGame());
  }
}
