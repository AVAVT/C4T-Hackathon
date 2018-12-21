using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
  [SerializeField] private TMP_Text gameSpeedText;
  [SerializeField] private TMP_Text endTurnText;
  [SerializeField] private InputField turnInputField;
  [SerializeField] private Slider turnSlider;
  [SerializeField] private Button playButton;
  [SerializeField] private Button stopButton;
  [SerializeField] private List<float> listGameSpeed;
  [SerializeField] private Transform toggleControlPanelButton;

  public Action PlayLog { private get; set; }
  public Action StopLog { private get; set; }
  public Action NextTurn { private get; set; }
  public Action PrevTurn { private get; set; }
  public Action<float> ChangeGameSpeed { private get; set; }
  public Action<int> ChangeTurn { private get; set; }
  private int currentSpeed = 0;
  private int currentTurn = 0;
  private bool showingControlPanel = true;

  void Start()
  {
    turnInputField.text = 0 + "";
    SetCurrentSpeedText(currentSpeed);
  }

  public void SetCurrentSpeedText(int currentSpeed)
  {
    gameSpeedText.text = $"x{listGameSpeed[currentSpeed]}";
  }

  public void DisplayCurrentTurn(int currentTurn)
  {
    this.currentTurn = currentTurn;
    turnSlider.value = currentTurn;
    turnInputField.text = currentTurn.ToString();
  }

  public void DisplayGameInfo(int endTurn)
  {
    turnSlider.maxValue = endTurn;
    endTurnText.text = endTurn.ToString();
  }

  //-----------------------------------------------Button methods----------------------------------------------------
  public void NextButtonClick()
  {
    StopButtonClick();
    NextTurn?.Invoke();
  }
  public void PrevButtonClick()
  {
    StopButtonClick();
    PrevTurn?.Invoke();
  }
  public void PlayButtonClick()
  {
    playButton.gameObject.SetActive(false);
    stopButton.gameObject.SetActive(true);
    PlayLog?.Invoke();
  }
  public void StopButtonClick()
  {
    playButton.gameObject.SetActive(true);
    stopButton.gameObject.SetActive(false);
    StopLog?.Invoke();
  }
  public void ChangeMultiplierButtonClick()
  {
    currentSpeed++;
    if (currentSpeed >= listGameSpeed.Count) currentSpeed = 0;
    gameSpeedText.text = $"x{listGameSpeed[currentSpeed]}";
    ChangeGameSpeed?.Invoke(listGameSpeed[currentSpeed]);
  }

  public void ChangeTurnBySlider()
  {
    if (currentTurn != turnSlider.value)
    {
      StopButtonClick();
      turnInputField.text = (int)turnSlider.value + "";
      ChangeTurn?.Invoke((int)turnSlider.value);
      currentTurn = (int)turnSlider.value;
    }
  }

  public void ChangeTurnByInputField()
  {
    if (int.Parse(turnInputField.text) >= 0)
    {
      StopButtonClick();
      turnSlider.value = int.Parse(turnInputField.text);
      ChangeTurn?.Invoke(int.Parse(turnInputField.text));
    }
  }

  public void ReplayButtonClick()
  {
    StopButtonClick();
    turnInputField.text = 0 + "";
    turnSlider.value = 0;
    ChangeTurn?.Invoke(0);
  }

  public void ToggleControlPanel()
  {
    if (showingControlPanel)
    {
      showingControlPanel = false;
      (transform as RectTransform).DOAnchorPosY(-135, 0.5f)
      .SetEase(Ease.InOutQuad)
      .OnStart(() => transform.GetComponent<CanvasGroup>().interactable = false)
      .OnComplete(() =>
      {
        gameObject.GetComponent<CanvasGroup>().interactable = true;
        toggleControlPanelButton.localScale = new Vector2(1, 1);
      });
    }
    else
    {
      showingControlPanel = true;
      (transform as RectTransform).DOAnchorPosY(0, 0.5f)
      .SetEase(Ease.InOutQuad)
      .OnStart(() => gameObject.GetComponent<CanvasGroup>().interactable = false)
      .OnComplete(() =>
      {
        SetTogglePanelButton();
      });
    }
  }
  public void SetTogglePanelButton()
  {
    gameObject.GetComponent<CanvasGroup>().interactable = true;
    toggleControlPanelButton.localScale = new Vector2(1, -1);
  }
}