using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour
{
  public IPythonInterpreter pythonManager;
  public IOutputUI uiManager;

  //turn base
  private int turn;

  void Start()
  {
  }

  public void ButtonPlayClick()
  {
    if (pythonManager.IsHavingAllFiles())
    {
      //start engine
      pythonManager.InitEngine();

      //call all AI's do_start methods
      for (int i = 0; i < 6; i++)
      {
        pythonManager.WaitAIResponse(i, GetClassnameByIndex(i), "do_start");
      }
    }
    else
    {
      Debug.Log("Please provide all required AI file for all characters!");
    }
  }

  public string GetClassnameByIndex(int index)
  {
    if (index == 0 || index == 3) return "Planter";
    else if (index == 1 || index == 4) return "Harvester";
    else return "Worm";
  }
}
