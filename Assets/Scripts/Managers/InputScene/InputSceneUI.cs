using System;
using System.Collections;
using GracesGames.SimpleFileBrowser.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class InputSceneUI : MonoBehaviour, IInputSceneUI
{
  [Header("UI Components")]
  [SerializeField] private GameObject startPanel;
  [SerializeField] private GameObject inputPanel;
  [SerializeField] private GameObject playRecordPanel;
  [SerializeField] private GameObject recordingPanel;
  [SerializeField] private GameObject loadingPanel;
  [Header("Input Panel")]
  [SerializeField] private InputField[] characterInputNames;
  [SerializeField] private Text[] characterPaths;

  [Header("Recording Panel")]
  [SerializeField] private Text outputText;
  [SerializeField] private Image recordingBar;
  [SerializeField] private TMP_Text recordingProcessText;
  [Header("Loading Panel")]
  [SerializeField] private Image loadingBar;
  [SerializeField] private TMP_Text loadingProcessText;

  [Header("File loader")]
  public GameObject FileBrowserPrefab;
  public string[] FileExtensions;
  public bool PortraitMode;

  public Action StartGame
  {
    set
    {
      startGame = value;
    }
  }
  private Action startGame;

  private void SavePlayerName()
  {
    for (int i = 0; i < characterInputNames.Length; i++)
    {
      if (!String.IsNullOrEmpty(characterInputNames[i].text))
        PlayerPrefs.SetString($"Player{i}Name", characterInputNames[i].text);
      else
        PlayerPrefs.SetString($"Player{i}Name", $"Player {i}");
    }
  }

  public void ShowOutputText(string text)
  {
    outputText.text += text + "\n";
  }

  public void ShowPathByIndex(int index, string path)
  {
    characterPaths[index].text = path;
  }

  //--------------------------------- Read file --------------------------------------
  private void OpenFileBrowser()
  {
    GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
    fileBrowserObject.name = "FileBrowser";

    FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
    fileBrowserScript.SetupFileBrowser(PortraitMode ? ViewMode.Portrait : ViewMode.Landscape, $"{Application.streamingAssetsPath}/logs");

    fileBrowserScript.OpenFilePanel(FileExtensions);
    // Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
    fileBrowserScript.OnFileSelect += LoadFileUsingPath;
  }
  // Loads a file using a path
  private void LoadFileUsingPath(string path)
  {
    if (!String.IsNullOrEmpty(path))
    {
      PlayerPrefs.SetString("LogPath", path);
      StartCoroutine(StartLoadingPlayScene());
    }
    else
    {
      Debug.Log("Invalid path given");
    }
  }

  //-------------------------------------- Button methods -----------------------------------------
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
    StartCoroutine(StartLoadingPlayScene());
  }

  public void ChooseRecordButtonClick()
  {
    OpenFileBrowser();
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
    recordingPanel.SetActive(true);
    startGame?.Invoke();
  }

  public void ShowRecordingProcess(float process)
  {
    recordingBar.fillAmount = process;
    recordingProcessText.text = $"{Mathf.FloorToInt(process * 100)}%";
  }
}