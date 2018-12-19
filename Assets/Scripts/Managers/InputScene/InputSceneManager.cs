using System;
using UnityEngine;

public class InputSceneManager: MonoBehaviour
{
  public GrpcInputManager grpcInputManager;
  public InputSceneUI uiManager;
  
  void Awake()
  {
    BindManagers();
  }

  private void BindManagers()
  {
    grpcInputManager.uiManager = uiManager;
  }
}