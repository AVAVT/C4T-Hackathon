using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayRecordUI : MonoBehaviour, IPlaySceneUI
{
  [Header("UI Components")]
  [SerializeField] private GameObject loadPanel;
  [SerializeField] private GameObject resultPanel;
  [SerializeField] private GameObject playRecordPanel;

  [Header("Control panel")]
  [SerializeField] private TMP_Text gameSpeedText;
  [SerializeField] private TMP_Text endTurnText;
  [SerializeField] private InputField turnInputField;
  [SerializeField] private Slider turnSlider;
  [SerializeField] private Button playButton;
  [SerializeField] private Button stopButton;
  [SerializeField] private List<float> listGameSpeed;
  [Header("Score panel")]
  [SerializeField] private TMP_Text redScoreText;
  [SerializeField] private TMP_Text blueScoreText;
  [SerializeField] private Image redScoreImage;
  [Header("Team panels")]
  [SerializeField] private GameObject redTeamPanel;
  [SerializeField] private GameObject blueTeamPanel;
  [SerializeField] private List<TMP_Text> listNameText;
  [Header("Result panel")]
  [SerializeField] private TMP_Text resultRedTeamText;
  [SerializeField] private TMP_Text resultBlueTeamText;
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

  void Start()
  {
    turnInputField.text = 0 + "";
    gameSpeedText.text = $"x{listGameSpeed[currentSpeed]}";
  }
  public void StartPlaying()
  {
    loadPanel.SetActive(false);
    playRecordPanel.SetActive(true);
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
        performedAction1[team * 3 + i].text += lastTurnCharacters[(Team)team][(CharacterRole)i].performAction1 + "";
        performedAction2[team * 3 + i].text += lastTurnCharacters[(Team)team][(CharacterRole)i].performAction2 + "";
      }
    }
    turnSlider.maxValue = this.endTurn;
    //find win team
    if (logs[logs.Count - 1].serverGameState.blueScore == 0 && logs[logs.Count - 1].serverGameState.redScore == 0)
    {
      resultBlueTeamText.text += " - Draw!";
      resultRedTeamText.text += " - Draw!";
    }
    else
    {
      int checkTurn = endTurn;
      while(logs[checkTurn].serverGameState.blueScore == logs[checkTurn].serverGameState.redScore)
      {
        checkTurn++;
        if(logs[checkTurn].serverGameState.blueScore > logs[checkTurn].serverGameState.redScore) winTeam = Team.Blue;
        else if(logs[checkTurn].serverGameState.blueScore > logs[checkTurn].serverGameState.redScore) winTeam = Team.Red;
      }
      if(winTeam == Team.Blue) resultBlueTeamText.text += " - Win!!!";
      else resultRedTeamText.text += " - Win!!!";
    }
  }

  public void ToggleResult(bool isShowingResult)
  {
    if (isShowingResult)
    {
      resultPanel.SetActive(true);
      blueTeamPanel.SetActive(false);
      redTeamPanel.SetActive(false);
    }
    else
    {
      resultPanel.SetActive(false);
      blueTeamPanel.SetActive(true);
      redTeamPanel.SetActive(true);
    }
  }

  //-----------------------------------------------Button methods----------------------------------------------------
  public void NextButtonClick()
  {
    nextTurn?.Invoke();
  }
  public void PrevButtonClick()
  {
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
}
