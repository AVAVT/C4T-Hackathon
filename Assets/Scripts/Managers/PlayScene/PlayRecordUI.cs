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
  [SerializeField] private GameObject resultPanel;
  [SerializeField] private GameObject controlPanel;
  [SerializeField] private GameObject scorePanel;
  [SerializeField] private TMP_Text countdownText;
  [SerializeField] private GameObject grid;

  [Header("Control panel")]
  [SerializeField] private TMP_Text gameSpeedText;
  [SerializeField] private TMP_Text endTurnText;
  [SerializeField] private InputField turnInputField;
  [SerializeField] private Slider turnSlider;
  [SerializeField] private Button playButton;
  [SerializeField] private Button stopButton;
  [SerializeField] private List<float> listGameSpeed;
  [SerializeField] private Transform toggleControlPanelButton;

  [Header("Score panel")]
  [SerializeField] private TMP_Text redScoreText;
  [SerializeField] private TMP_Text blueScoreText;
  [SerializeField] private Image redScoreImage;
  [SerializeField] private Image compareImage;
  [SerializeField] private Sprite redGreaterImage;
  [SerializeField] private Sprite blueGreaterImage;

  [Header("Team panels")]
  [SerializeField] private GameObject redTeamPanel;
  [SerializeField] private GameObject blueTeamPanel;
  [SerializeField] private List<TMP_Text> listNameText;
  [SerializeField] private List<TMP_Text> listStatusText;
  [SerializeField] private List<Image> listFadeAva;

  [Header("Result panel")]
  [SerializeField] private Text resultRedTeamText;
  [SerializeField] private Text resultBlueTeamText;
  [SerializeField] private TMP_Text[] resultCharacterNames;
  [SerializeField] private TMP_Text[] performedAction1;
  [SerializeField] private TMP_Text[] performedAction2;  

  //private actions
  private int currentSpeed = 1;
  public Action PlayLog { set { playLog = value; } }
  public Action StopLog { set { stopLog = value; } }
  public Action NextTurn { set { nextTurn = value; } }
  public Action PrevTurn { set { prevTurn = value; } }
  public Action<float> ChangeGameSpeed { set { changeGameSpeed = value; } }
  public Action<int> ChangeTurn { set { changeTurn = value; } }

  private Action playLog, stopLog, nextTurn, prevTurn;
  private Action<float> changeGameSpeed;
  private Action<int> changeTurn;

  private int endTurn;
  private List<string> listNames;
  private Team winTeam;
  private bool showingControlPanel = true;

  void Start()
  {
    turnInputField.text = 0 + "";
    gameSpeedText.text = $"x{listGameSpeed[currentSpeed]}";
    StartAnimation();
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
      controlPanel.GetComponent<CanvasGroup>().interactable = true;
      toggleControlPanelButton.localScale = new Vector2(1, -1);
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
    playLog?.Invoke();
  }

  public void DisplayTurnInfo(int currentTurn, int blueScore, int redScore)
  {
    turnInputField.text = currentTurn.ToString();
    redScoreText.text = redScore.ToString();
    blueScoreText.text = blueScore.ToString();
    if (redScore != 0 || blueScore != 0)
      redScoreImage.fillAmount = redScore / (redScore + blueScore);
    else
      redScoreImage.fillAmount = 0.5f;

    compareImage.gameObject.SetActive(true);
    if (blueScore == redScore) compareImage.gameObject.SetActive(false);
    else if (blueScore > redScore) compareImage.sprite = blueGreaterImage;
    else compareImage.sprite = redGreaterImage;

    compareImage.rectTransform.anchoredPosition = compareImage.rectTransform.anchoredPosition.WithX(redScoreImage.fillAmount * redScoreImage.rectTransform.sizeDelta.x);
    if (compareImage.rectTransform.anchoredPosition.x <= compareImage.rectTransform.sizeDelta.x / 2)
      compareImage.rectTransform.anchoredPosition = compareImage.rectTransform.anchoredPosition.WithX(compareImage.rectTransform.sizeDelta.x / 2);
    var maxX = redScoreImage.rectTransform.sizeDelta.x - compareImage.rectTransform.sizeDelta.x / 2;
    if (compareImage.rectTransform.anchoredPosition.x >= maxX)
      compareImage.rectTransform.anchoredPosition = compareImage.rectTransform.anchoredPosition.WithX(maxX);
  }

  public void DisplayGameInfo(List<string> listNames, List<RecordModel> logs)
  {
    this.listNames = listNames;
    endTurn = logs.Count - 1;

    endTurnText.text = this.endTurn.ToString();
    for (int i = 0; i < this.listNames.Count; i++)
    {
      this.listNameText[i].text = listNames[i];
    }
    for (int i = 0; i < this.listNames.Count; i++)
    {
      resultCharacterNames[i].text = listNames[i];
    }
    var lastTurnCharacters = logs[logs.Count - 1].serverGameState.characters;
    for (int team = 0; team < 2; team++)
    {
      for (int i = 0; i < 3; i++)
      {
        // TODO Tung
        // performedAction1[team * 3 + i].text = lastTurnCharacters[(Team)team][(CharacterRole)i].performAction1 + "";
        // performedAction2[team * 3 + i].text = lastTurnCharacters[(Team)team][(CharacterRole)i].performAction2 + "";
      }
    }
    turnSlider.maxValue = this.endTurn;
    //find win team
    if (logs[logs.Count - 1].serverGameState.blueScore == 0 && logs[logs.Count - 1].serverGameState.redScore == 0)
    {
      resultBlueTeamText.text = "Draw";
      resultBlueTeamText.GetComponent<Outline>().effectColor = new Color(248, 124, 3);
      resultRedTeamText.text = "Draw";
      resultRedTeamText.GetComponent<Outline>().effectColor = new Color(248, 124, 3);
    }
    else
    {
      int checkTurn = endTurn;
      while (logs[checkTurn].serverGameState.blueScore == logs[checkTurn].serverGameState.redScore)
      {
        checkTurn++;
        if (logs[checkTurn].serverGameState.blueScore > logs[checkTurn].serverGameState.redScore) winTeam = Team.Blue;
        else if (logs[checkTurn].serverGameState.blueScore > logs[checkTurn].serverGameState.redScore) winTeam = Team.Red;
      }
      resultBlueTeamText.GetComponent<Outline>().effectColor = new Color(18, 151, 254);
      resultRedTeamText.GetComponent<Outline>().effectColor = new Color(218, 0, 52);
      if (winTeam == Team.Blue)
      {
        resultBlueTeamText.text = "Victory!";
        resultRedTeamText.text = "Defeated";
      }
      else
      {
        resultRedTeamText.text = "Victory!";
        resultBlueTeamText.text = "Defeated";
      }
    }
  }

  public void DisplayCharacterStatus(int index, string status)
  {
    listStatusText[index].text = status;
    if(status == "Time out" || status == "Crashed")
      listFadeAva[index].gameObject.SetActive(true);
  }

  public void ToggleResult(bool isShowingResult)
  {
    if (isShowingResult)
    {
      resultPanel.SetActive(true);
      blueTeamPanel.SetActive(false);
      redTeamPanel.SetActive(false);
      grid.SetActive(false);

    }
    else
    {
      resultPanel.SetActive(false);
      blueTeamPanel.SetActive(true);
      redTeamPanel.SetActive(true);
      grid.SetActive(true);
    }
  }

  //-----------------------------------------------Button methods----------------------------------------------------
  public void NextButtonClick()
  {
    StopButtonClick();
    nextTurn?.Invoke();
  }
  public void PrevButtonClick()
  {
    StopButtonClick();
    prevTurn?.Invoke();
  }
  public void PlayButtonClick()
  {
    playButton.gameObject.SetActive(false);
    stopButton.gameObject.SetActive(true);
    playLog?.Invoke();
  }
  public void StopButtonClick()
  {
    playButton.gameObject.SetActive(true);
    stopButton.gameObject.SetActive(false);
    stopLog?.Invoke();
  }
  public void ChangeMultiplierButtonClick()
  {
    currentSpeed++;
    if (currentSpeed >= listGameSpeed.Count) currentSpeed = 0;
    gameSpeedText.text = $"x{listGameSpeed[currentSpeed]}";
    changeGameSpeed?.Invoke(listGameSpeed[currentSpeed]);
  }

  public void ChangeTurnBySlider()
  {
    StopButtonClick();
    turnInputField.text = (int)turnSlider.value + "";
    changeTurn?.Invoke((int)turnSlider.value);
  }

  public void ChangeTurnByInputField()
  {
    if (int.Parse(turnInputField.text) >= 0)
    {
      StopButtonClick();
      turnSlider.value = int.Parse(turnInputField.text);
      changeTurn?.Invoke(int.Parse(turnInputField.text));
    }
  }

  public void ReplayButtonClick()
  {
    StopButtonClick();
    turnInputField.text = 0 + "";
    turnSlider.value = 0;
    changeTurn?.Invoke(0);
  }

  public void ToggleControlPanel()
  {
    if (showingControlPanel)
    {
      showingControlPanel = false;
      (controlPanel.transform as RectTransform).DOAnchorPosY(-135, 0.5f)
      .SetEase(Ease.InOutQuad)
      .OnStart(() => controlPanel.GetComponent<CanvasGroup>().interactable = false)
      .OnComplete(() =>
      {
        controlPanel.GetComponent<CanvasGroup>().interactable = true;
        toggleControlPanelButton.localScale = new Vector2(1, 1);
      });
    }
    else
    {
      showingControlPanel = true;
      (controlPanel.transform as RectTransform).DOAnchorPosY(0, 0.5f)
      .SetEase(Ease.InOutQuad)
      .OnStart(() => controlPanel.GetComponent<CanvasGroup>().interactable = false)
      .OnComplete(() =>
      {
        controlPanel.GetComponent<CanvasGroup>().interactable = true;
        toggleControlPanelButton.localScale = new Vector2(1, -1);
      });
    }
  }

  public void QuitButtonClick()
  {
    SceneManager.LoadScene("InputScene");
  }
}
