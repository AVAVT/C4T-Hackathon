using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
  public Action EnableInputPanel {private get; set; }
  [SerializeField] private Image loadingBar;
  [SerializeField] private TMP_Text loadingProcessText;
  [SerializeField] private TMP_Text tipsText;
  [SerializeField] private string[] tips;

  [SerializeField] private GameObject recordErrorPanel;
  [SerializeField] private TMP_Text outputText;

  public void ShowRecordingProcess(int currentTurn, int gameLength)
  {
    loadingBar.fillAmount = (float)currentTurn / (float)gameLength;
    loadingProcessText.text = $"Recording Turn: {currentTurn}";
    if (tips.Length != 0) tipsText.text = $"Tips: {tips.RandomItem()}";
  }

  public void ShowErrorPanel(string errorMessage)
  {
    outputText.text = errorMessage;
    recordErrorPanel.SetActive(true);
  }

  public void BackFromRecordPanel()
  {
    outputText.text = "";
    recordErrorPanel.SetActive(false);
    EnableInputPanel?.Invoke();
  }

  public IEnumerator StartLoadingPlayScene()
  {
    this.gameObject.SetActive(true);
    var process = SceneManager.LoadSceneAsync("PlayScene");
    while (!process.isDone)
    {
      loadingBar.fillAmount = process.progress;
      loadingProcessText.text = $"{Mathf.FloorToInt(process.progress * 100)}%";
      yield return null;
    }
  }
}