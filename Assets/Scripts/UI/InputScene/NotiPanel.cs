using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotiPanel : MonoBehaviour
{
  [SerializeField] private Image notiPanel;
  [SerializeField] private TMP_Text notiText;
  [SerializeField] private TMP_Text noticeText;

  public IEnumerator ShowNotiPanel(string text, float delay, float duration)
  {
    DOTween.Complete(notiPanel);
    DOTween.Complete(notiText);
    DOTween.Complete(noticeText);

    notiPanel.color = ColorExtension.SetColorAlpha(notiPanel.color, 1);
    notiText.color = ColorExtension.SetColorAlpha(notiText.color, 1);
    noticeText.color = ColorExtension.SetColorAlpha(notiText.color, 1);
    notiPanel.gameObject.SetActive(true);
    notiText.text = text;

    noticeText.DOColor(ColorExtension.SetColorAlpha(noticeText.color, 0), duration).SetDelay(delay);
    notiPanel.DOColor(ColorExtension.SetColorAlpha(notiPanel.color, 0), duration).SetDelay(delay);
    notiText.DOColor(ColorExtension.SetColorAlpha(notiText.color, 0), duration)
    .SetDelay(delay)
    .OnComplete(() =>
    {
      notiPanel.gameObject.SetActive(false);
    });
    yield return new WaitUntil(() => Input.anyKeyDown);
    notiPanel.gameObject.SetActive(false);
  }
}