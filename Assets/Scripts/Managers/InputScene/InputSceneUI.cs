using System;
using UnityEngine;

public class InputSceneUI : MonoBehaviour, IInputSceneUI
{
  [Header("UI Components")]
  [SerializeField] private StartPanel startPanel;
  [SerializeField] private PlayRecordPanel playRecordPanel;
  [SerializeField] private SettingPanel settingPanel;
  [SerializeField] private InputPanel inputPanel;
  [SerializeField] private LoadingPanel loadingPanel;
  [SerializeField] private NotiPanel notiPanel;

  public Action<int, bool> SetIsBot
  {
    set
    {
      setIsBot = value;
    }
  }
  private Action<int, bool> setIsBot;
  public Action<bool> ChangeMap
  {
    set
    {
      changeMap = value;
    }
  }
  private Action<bool> changeMap;
  public Action StartGame
  {
    set
    {
      startGame = value;
    }
  }
  private Action startGame;

  //---------------------------------- Bind panels -----------------------------------
  void Start()
  {
    BindPanels();

    CallChangeMap(true);
    CallChangeMap(false);
  }

  private void BindPanels()
  {
    BindStartPanel();
    BindPlayRecordPanel();
    BindSettingPanel();
    BindInputPanel();
    BindLoadingPanel();
  }

  private void BindStartPanel()
  {
    startPanel.EnableInputPanel = ToInputPanel;
    startPanel.EnablePlayRecordPanel = ToPlayRecordPanel;
    startPanel.EnableSettingPanel = ToSettingPanel;
  }
  private void BindPlayRecordPanel()
  {
    playRecordPanel.StartLoadingPlayScene = StartLoadingPlayScene;
    playRecordPanel.ShowNotiPanel = ShowNotiPanel;
    playRecordPanel.EnableStartPanel = ToStartPanel;
  }
  private void BindSettingPanel()
  {
    settingPanel.EnableStartPanel = ToStartPanel;
    settingPanel.EnableSettingPanel = ToSettingPanel;
  }
  private void BindInputPanel()
  {
    inputPanel.ShowNotiPanel = ShowNotiPanel;
    inputPanel.SetIsBot = CallIsSetBot;
    inputPanel.ChangeMap = CallChangeMap;
    inputPanel.StartGame = CallStartGame;
    inputPanel.EnableLoadingPanel = ToLoadingPanel;
    inputPanel.EnableStartPanel = ToStartPanel;
  }
  private void BindLoadingPanel()
  {
    loadingPanel.EnableInputPanel = ToInputPanel;
  }

  private void DisableAllPanel()
  {
    inputPanel.gameObject.SetActive(false);
    loadingPanel.gameObject.SetActive(false);
    playRecordPanel.gameObject.SetActive(false);
    settingPanel.gameObject.SetActive(false);
    startPanel.gameObject.SetActive(false);
  }
  private void ToStartPanel()
  {
    DisableAllPanel();
    startPanel.gameObject.SetActive(true);
  }
  private void ToPlayRecordPanel()
  {
    DisableAllPanel();
    playRecordPanel.gameObject.SetActive(true);
  }
  private void ToInputPanel()
  {
    DisableAllPanel();
    inputPanel.gameObject.SetActive(true);
  }
  private void ToLoadingPanel()
  {
    DisableAllPanel();
    loadingPanel.gameObject.SetActive(true);
  }
  private void ToSettingPanel()
  {
    DisableAllPanel();
    settingPanel.gameObject.SetActive(true);
    settingPanel.GetComponent<SettingPanel>().ResetSettingPanel();
  }

  private void CallIsSetBot(int index, bool isTrue)
  {
    setIsBot?.Invoke(index, isTrue);
  }
  private void CallStartGame()
  {
    startGame?.Invoke();
  }
  private void CallChangeMap(bool isNext)
  {
    changeMap?.Invoke(isNext);
  }
  
  //----------------------------------- Interface methods --------------------------------------------
  public void StartLoadingPlayScene()
  {
    StartCoroutine(loadingPanel.GetComponent<LoadingPanel>().StartLoadingScene("PlayScene"));
  }

  public void ShowRecordingProcess(int currentTurn, int gameLength)
  {
    loadingPanel.ShowRecordingProcess(currentTurn, gameLength);
  }

  public void ShowNotiPanel(string text, float delay, float duration)
  {
    StartCoroutine(notiPanel.ShowNotiPanel(text, delay, duration));
  }

  public void ShowRecordPanelWhenError(string errorMessage)
  {
    loadingPanel.GetComponent<LoadingPanel>().ShowErrorPanel(errorMessage);
  }

  public void ShowMapInfo(MapDisplayData mapInfo)
  {
    inputPanel.GetComponent<InputPanel>().ShowMapInfo(mapInfo);
  }
}