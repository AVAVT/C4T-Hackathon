using System;
using System.Collections;
using Crosstales.FB;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Diagnostics;
using System.IO;

public class InputSceneUI : MonoBehaviour, IInputSceneUI
{
  [Header("UI Components")]
  [SerializeField] private GameObject startPanel;
  [SerializeField] private GameObject inputPanel;
  [SerializeField] private GameObject playRecordPanel;
  [SerializeField] private GameObject recordingPanel;
  [SerializeField] private GameObject loadingPanel;
  [SerializeField] private GameObject settingPanel;
  [Header("Input Panel")]
  [SerializeField] private InputField[] characterInputNames;
  [SerializeField] private Image[] characterStatus;
  [SerializeField] private Image[] characterBrowseButtons;
  [SerializeField] private Sprite importButtonSprite, changeButtonSprite;
  [SerializeField] private Sprite unreadyStatusSprite, readyStatusSprite;
  [SerializeField] private Image NotiPanel;
  [SerializeField] private TMP_Text NotiText;

  [Header("Map info")]
  [SerializeField] private Image mapPreview;
  [SerializeField] private TMP_Text mapName;

  [Header("Recording Panel")]
  [SerializeField] private TMP_Text outputText;

  [Header("Loading Panel")]
  [SerializeField] private Image loadingBar;
  [SerializeField] private TMP_Text loadingProcessText;
  [SerializeField] private TMP_Text tipsText;
  [SerializeField] private string[] tips;
  [Header("Setting panel")]
  [SerializeField] private InputField pythonPathText;
  [SerializeField] private InputField saveLogPathText;
  [SerializeField] private TMP_Text errorText;
  [SerializeField] private GameObject btnSettingBack; //not show when first config

  public Action StartGame
  {
    set
    {
      startGame = value;
    }
  }

  public Action<int> LoadAIFolder
  {
    set
    {
      loadAIFolder = value;
    }
  }

  public bool HaveError
  {
    get
    {
      return haveError;
    }
  }

  public Action<bool> ChangeMap
  {
    set
    {
      changeMap = value;
    }
  }

  private Action startGame;
  private Action<int> loadAIFolder;
  private Action<bool> changeMap;
  private string errorMessage;
  private bool haveError = false;
  void Start()
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
        startPanel.SetActive(false);
        settingPanel.SetActive(true);
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

  private void SavePlayerName()
  {
    for (int i = 0; i < characterInputNames.Length; i++)
    {
      if (!String.IsNullOrEmpty(characterInputNames[i].text))
        PlayerPrefs.SetString($"Player{i}Name", characterInputNames[i].text);
      else
        PlayerPrefs.SetString($"Player{i}Name", $"Player {i + 1}");
    }
  }

  public void SaveErrorMessage(string text, bool haveError)
  {
    if (haveError)
    {
      this.haveError = true;
      errorMessage += text + Environment.NewLine + Environment.NewLine;
    }
    if (!haveError && !this.haveError) errorMessage += text + Environment.NewLine + Environment.NewLine;
  }

  //--------------------------------- Choose files --------------------------------------
  private void ChooseRecordBrowser()
  {
    var path = FileBrowser.OpenSingleFile("Choose log file to play!", Application.streamingAssetsPath + "/logs", "json");
    if (!String.IsNullOrEmpty(path))
    {
      PlayerPrefs.SetString("LogPath", path);
      StartCoroutine(StartLoadingPlayScene());
    }
    else
    {
      UnityEngine.Debug.Log("Invalid path given or folder does not contain main.py file!");
    }
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
      errorText.text = "Invalid path given or it is not a path to python.exe!";
      errorText.gameObject.SetActive(true);
      UnityEngine.Debug.Log("Invalid path given");
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

  public void ShowFileStatus(int index)
  {
    characterBrowseButtons[index].sprite = changeButtonSprite;
    characterStatus[index].sprite = readyStatusSprite;
  }

  public void ShowNotiPanel(string text, float delay, float duration)
  {
    DOTween.Complete(NotiPanel);
    DOTween.Complete(NotiText);

    NotiPanel.color = Utilities.SetColorAlpha(NotiPanel.color, 1);
    NotiText.color = Utilities.SetColorAlpha(NotiText.color, 1);
    NotiPanel.gameObject.SetActive(true);
    NotiText.text = text;

    NotiPanel.DOColor(Utilities.SetColorAlpha(NotiPanel.color, 0), duration).SetDelay(delay);
    NotiText.DOColor(Utilities.SetColorAlpha(NotiText.color, 0), duration)
    .SetDelay(delay)
    .OnComplete(() =>
    {
      NotiPanel.gameObject.SetActive(false);
    });
  }

  public void ShowMapInfo(Map mapInfo)
  {
    mapName.text = $"{mapInfo.MAP_NAME} ({mapInfo.MAP_WIDTH} x {mapInfo.MAP_HEIGHT})";
    mapPreview.sprite = mapInfo.MAP_PREVIEW;
  }

  //-------------------------------------- Button methods -----------------------------------------
  public void NextMapButtonClick()
  {
    changeMap?.Invoke(true);
  }
  public void PrevMapButtonClick()
  {
    changeMap?.Invoke(false);
  }
  public void LoadAIFolderButtonClick(int index)
  {
    loadAIFolder?.Invoke(index);
  }
  public void StartGameButtonClick()
  {
    //TODO: Transaction
    startPanel.SetActive(false);
    inputPanel.SetActive(true);
  }
  public void PlayRecordButtonClick()
  {
    //TODO: Transaction
    startPanel.SetActive(false);
    playRecordPanel.SetActive(true);
  }
  public void ExitButtonClick()
  {
    Application.Quit();
  }

  public void LastRecordButtonClick()
  {
    if (File.Exists(PlayerPrefs.GetString("LogPath")))
    {
      playRecordPanel.SetActive(false);
      StartCoroutine(StartLoadingPlayScene());
    }
    else
      ShowNotiPanel("Have not recorded any log file or the file has been deleted!", 2, 1);
  }

  public void ChooseRecordButtonClick()
  {
    playRecordPanel.SetActive(false);
    ChooseRecordBrowser();
  }

  public IEnumerator StartLoadingPlayScene()
  {
    loadingPanel.SetActive(true);
    var process = SceneManager.LoadSceneAsync("PlayScene");
    while (!process.isDone)
    {
      loadingBar.fillAmount = process.progress;
      loadingProcessText.text = $"{Mathf.FloorToInt(process.progress * 100)}%";
      yield return null;
    }
  }

  public void PlayButtonClick()
  {
    SavePlayerName();
    inputPanel.SetActive(false);
    loadingPanel.SetActive(true);
    startGame?.Invoke();
  }

  public void ShowRecordingProcess(int currentTurn, int gameLength)
  {
    loadingBar.fillAmount = (float)currentTurn / (float)gameLength;
    loadingProcessText.text = $"Recording Turn: {currentTurn}";
    if (tips.Length != 0) tipsText.text = $"Tips: {tips.RandomItem()}";
  }

  public void BackFromInputPanel()
  {
    inputPanel.SetActive(false);
    startPanel.SetActive(true);
  }

  public void BackFromPlayRecordPanel()
  {
    playRecordPanel.SetActive(false);
    startPanel.SetActive(true);
  }

  public void SettingButtonClick()
  {
    errorText.gameObject.SetActive(false);
    btnSettingBack.gameObject.SetActive(true);
    startPanel.SetActive(false);
    settingPanel.SetActive(true);
  }

  public void BackFromSettingPanel()
  {
    startPanel.SetActive(true);
    settingPanel.SetActive(false);
  }

  public void BrowsePythonPathButtonClick()
  {
    ChoosePythonBrowser();
  }

  public void BrowseSaveLogPathButtonClick()
  {
    ChooseLogPathBrowser();
  }

  public void ShowRecordPanelWhenError()
  {
    outputText.text = errorMessage;
    recordingPanel.SetActive(true);
  }
  public void BackFromRecordPanel()
  {
    outputText.text = "";
    recordingPanel.SetActive(false);
    loadingPanel.SetActive(false);
    inputPanel.SetActive(true);
  }
}