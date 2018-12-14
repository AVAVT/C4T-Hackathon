using System.Collections;
using UnityEngine;

public class DisplayCharacter : MonoBehaviour
{
  public GameObject characterNoti;
  private Coroutine showNotiCoroutine;

  IEnumerator ShowCharacterNoti(float showTime, Sprite emoSprite)
  {
    characterNoti.GetComponent<SpriteRenderer>().sprite = emoSprite;
    characterNoti.SetActive(true);
    yield return new WaitForSeconds(showTime);
    characterNoti.SetActive(false);
  }

  public void StartShowCharacterNoti(float showTime, Sprite emoSprite)
  {
    if(showNotiCoroutine != null) StopCoroutine(showNotiCoroutine);
    showNotiCoroutine = StartCoroutine(ShowCharacterNoti(showTime, emoSprite));
  }
}