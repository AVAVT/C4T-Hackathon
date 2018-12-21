using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamPanel : MonoBehaviour
{
  public GameObject redTeamPanel;
  public GameObject blueTeamPanel;
  [SerializeField] private List<TMP_Text> listNameText;
  [SerializeField] private List<TMP_Text> listStatusText;
  [SerializeField] private List<Image> listFadeAva;

  public void DisplayGameInfo(List<string> listNames)
  {
    for (int i = 0; i < listNames.Count; i++)
    {
      listNameText[i].text = listNames[i];
    }
  }

  public void DisplayCharacterStatus(int index, string status)
  {
    listStatusText[index].text = status;
    if (status == "Time out" || status == "Crashed")
      listFadeAva[index].gameObject.SetActive(true);
  }

  
}