using System;
using System.Collections;
using Crosstales.FB;
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

  private Action startGame;
  private Action<int> loadAIFolder;

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
    var path = FileBrowser.OpenSingleFile("Open log file", Application.streamingAssetsPath +"/logs", "json");
    LoadFileUsingPath(path);
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
}