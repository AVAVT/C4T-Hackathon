using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayRecordUI : MonoBehaviour, IPlaySceneUI
{
  [Header("UI Components")]
  [SerializeField] private TeamPanel teamPanel;
  [SerializeField] private ResultPanel resultPanel;
  [SerializeField] private ControlPanel controlPanel;
  [SerializeField] private ScorePanel scorePanel;
  [SerializeField] private TMP_Text countdownText;
  [SerializeField] private GameObject grid;

  private GameObject blueTeamPanel, redTeamPanel;

  //private actions
  public Action<bool> PlayLog { set { playLog = value; } }
  public Action StopLog { set { stopLog = value; } }
  public Action NextTurn { set { nextTurn = value; } }
  public Action PrevTurn { set { prevTurn = value; } }
  public Action<float> ChangeGameSpeed { set { changeGameSpeed = value; } }
  public Action<int> ChangeTurn { set { changeTurn = value; } }

  private Action<bool> playLog;
  private Action stopLog, nextTurn, prevTurn;
  private Action<float> changeGameSpeed;
  private Action<int> changeTurn;

  private int endTurn;
  void Start()
  {
    BindMethods();
    blueTeamPanel = teamPanel.blueTeamPanel;
    redTeamPanel = teamPanel.redTeamPanel;
    StartAnimation();
  }

  private void BindMethods()
  {
    controlPanel.PlayLog = CallPlayLog;
    controlPanel.StopLog = CallStopLog;
    controlPanel.NextTurn = CallNextTurn;
    controlPanel.PrevTurn = CallPrevTurn;
    controlPanel.ChangeGameSpeed = CallChangeGameSpeed;
    controlPanel.ChangeTurn = CallChangeTurn;
  }
  private void CallPlayLog()
  {
    playLog?.Invoke(true);
  }
  private void CallStopLog()
  {
    stopLog?.Invoke();
  }

  private void CallNextTurn()
  {
    nextTurn?.Invoke();
  }
  private void CallPrevTurn()
  {
    prevTurn?.Invoke();
  }
  private void CallChangeGameSpeed(float speed)
  {
    changeGameSpeed?.Invoke(speed);
  }
  private void CallChangeTurn(int currentTurn)
  {
    changeTurn?.Invoke(currentTurn);
  }

  private void StartAnimation()
  {
    var sequence = DOTween.Sequence();
    sequence.Append((scorePanel.transform as RectTransform).DOAnchorPosY(-175, 0.5f).From().SetEase(Ease.OutBack));
    sequence.Append((blueTeamPanel.transform as RectTransform).DOAnchorPosX(-350, 0.5f).From().SetEase(Ease.OutBack));
    sequence.Join((redTeamPanel.transform as RectTransform).DOAnchorPosX(350, 0.5f).From().SetEase(Ease.OutBack));
    sequence.Append((controlPanel.transform as RectTransform).DOAnchorPosY(-175, 0.5f).From()
    .SetEase(Ease.OutQuad)
    .OnStart(() => controlPanel.GetComponent<CanvasGroup>().interactable = false)
    .OnComplete(() =>
    {
      controlPanel.SetTogglePanelButton();
      StartCoroutine(CountDownText());
    }));
  }

  private IEnumerator CountDownText()
  {
    countdownText.gameObject.SetActive(true);
    float index = 4;
    while (index > 0)
    {
      index -= Time.deltaTime;
      countdownText.text = Mathf.FloorToInt(index).ToString();
      if (index <= 1) countdownText.text = "Go!";
      yield return null;
    }
    countdownText.gameObject.SetActive(false);
    playLog?.Invoke(false);
  }

  public void DisplayTurnInfo(int currentTurn, int blueScore, int redScore)
  {
    controlPanel.DisplayCurrentTurn(currentTurn);
    scorePanel.DisplayScoreInfo(blueScore, redScore);
  }

  public void DisplayGameInfo(List<string> listNames, List<RecordModel> logs)
  {
    endTurn = logs.Count - 1;
    controlPanel.DisplayGameInfo(endTurn);
    teamPanel.DisplayGameInfo(listNames);
    resultPanel.DisplayResultInfo(listNames, logs, endTurn);
  }

  public void DisplayCharacterStatus(int index, string status)
  {
    teamPanel.DisplayCharacterStatus(index, status);
  }

  public void ToggleResult(bool isShowingResult)
  {
    if (isShowingResult)
    {
      resultPanel.gameObject.SetActive(true);
      blueTeamPanel.SetActive(false);
      redTeamPanel.SetActive(false);
      // grid.SetActive(false);

    }
    else
    {
      resultPanel.gameObject.SetActive(false);
      blueTeamPanel.SetActive(true);
      redTeamPanel.SetActive(true);
      // grid.SetActive(true);
    }
  }

  public void QuitButtonClick()
  {
    SceneManager.LoadScene("InputScene");
  }
}
