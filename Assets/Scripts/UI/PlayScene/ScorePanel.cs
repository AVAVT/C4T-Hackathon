using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel : MonoBehaviour
{
  [SerializeField] private TMP_Text redScoreText;
  [SerializeField] private TMP_Text blueScoreText;
  [SerializeField] private Image redScoreImage;
  [SerializeField] private Image compareImage;
  [SerializeField] private Sprite redGreaterImage;
  [SerializeField] private Sprite blueGreaterImage;

  public void DisplayScoreInfo(int blueScore, int redScore)
  {
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
}