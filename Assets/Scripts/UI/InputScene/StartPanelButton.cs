using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanelButton : MonoBehaviour
{
  public GameObject buttonHighlight;

  public void OnMouseEnter()
  {
    buttonHighlight.SetActive(true);
  }

  public void OnMouseExit()
  {
    buttonHighlight.SetActive(false);
  }
}
