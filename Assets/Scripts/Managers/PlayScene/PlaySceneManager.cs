using UnityEngine;

public class PlaySceneManager:MonoBehaviour
{
  public DisplayRecordManager displayRecordManager;
  public PlayRecordUI uiManager;

  /// <summary>
  /// Awake is called when the script instance is being loaded.
  /// </summary>
  void Awake()
  {
    displayRecordManager.uiManager = uiManager;
  }
}