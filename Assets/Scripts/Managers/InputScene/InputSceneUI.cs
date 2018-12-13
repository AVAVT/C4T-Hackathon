using System;
using System.Collections;
using Crosstales.FB;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
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

  public string ErrorMessage
  {
    get
    {
      return errorMessage;
    }
  }

  private Action startGame;
  private Action<int> loadAIFolder;
  private string errorMessage;
  private bool haveError = false;
  void Start()
  {
// #if UNITY_EDITOR
//     PlayerPrefs.DeleteKey("HaveConfig");
// #endif

    if (PlayerPrefs.GetInt("HaveConfig", 0) == 0)
    {
      startPanel.SetActive(false);
      settingPanel.SetActive(true);
      btnSettingBack.SetActive(false);
    }
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
    if(haveError) this.haveError = true;
    errorMessage += text + "\n";
    // outputText.text += text + "\n";
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
      Debug.Log("Invalid path given or folder does not contain main.py file!");
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
      errorText.text = "Setup successfully!";
      errorText.gameObject.SetActive(true);
      btnSettingBack.SetActive(true);
    }
    else
    {
      errorText.text = "Invalid path given or it is not a path to python.exe!";
      errorText.gameObject.SetActive(true);
      Debug.Log("Invalid path given");
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
    .OnComplete(() => {
      NotiPanel.gameObject.SetActive(false);
    });
  }

  //-------------------------------------- Button methods -----------------------------------------
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
    playRecordPanel.SetActive(false);
    StartCoroutine(StartLoadingPlayScene());
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
    loadingBar.fillAmount = (float)currentTurn/(float)gameLength;
    loadingProcessText.text = $"Recording Turn: {currentTurn}";
    if(tips.Length != 0) tipsText.text = $"Tips: {tips.RandomItem()}";
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

  public void ShowRecordPanelWhenError()
  {
    outputText.text = errorMessage;
    recordingPanel.SetActive(true);
  }
  public void BackFromRecordPanel()
  {
    recordingPanel.SetActive(false);
    loadingPanel.SetActive(false);
    inputPanel.SetActive(true);
  }
}