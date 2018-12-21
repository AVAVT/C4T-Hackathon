using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
  [SerializeField] private Text resultRedTeamText;
  [SerializeField] private Text resultBlueTeamText;
  [SerializeField] private TMP_Text[] resultCharacterNames;
  [SerializeField] private TMP_Text[] performedAction1;
  [SerializeField] private TMP_Text[] performedAction2;
  private Team winTeam;

  public void DisplayResultInfo(List<string> listNames, List<RecordModel> logs, int endTurn)
  {
    for (int i = 0; i < listNames.Count; i++)
    {
      resultCharacterNames[i].text = listNames[i];
    }
    var lastTurnCharacters = logs[logs.Count - 1].serverGameState.characters;
    var planters = lastTurnCharacters.GetItemsBy(CharacterRole.Planter);
    var harvesters = lastTurnCharacters.GetItemsBy(CharacterRole.Harvester);
    var worms = lastTurnCharacters.GetItemsBy(CharacterRole.Worm);

    foreach (var planter in planters)
    {
      int index = (int)planter.team * 3 + (int)planter.characterRole;
      performedAction1[index].text = planter.numTreePlanted + "";
      performedAction2[index].text = planter.numWormCaught + "";
    }
    foreach (var harvester in harvesters)
    {
      int index = (int)harvester.team * 3 + (int)harvester.characterRole;
      performedAction1[index].text = harvester.numFruitHarvested + "";
      performedAction2[index].text = harvester.numFruitDelivered + "";
    }
    foreach (var worm in worms)
    {
      int index = (int)worm.team * 3 + (int)worm.characterRole;
      performedAction1[index].text = worm.numTreeDestroyed + "";
      performedAction2[index].text = worm.numHarvesterScared + "";
    }

    //find win team
    if (logs[logs.Count - 1].serverGameState.blueScore == 0 && logs[logs.Count - 1].serverGameState.redScore == 0)
    {
      resultBlueTeamText.text = "Draw";
      resultBlueTeamText.GetComponent<Outline>().effectColor = new Color(248f/255f, 124f/255f, 3f/255f);
      resultRedTeamText.text = "Draw";
      resultRedTeamText.GetComponent<Outline>().effectColor = new Color(248f/255f, 124f/255f, 3f/255f);
    }
    else
    {
      int checkTurn = endTurn;
      while (logs[checkTurn].serverGameState.blueScore == logs[checkTurn].serverGameState.redScore)
      {
        checkTurn--;
        if (logs[checkTurn].serverGameState.blueScore > logs[checkTurn].serverGameState.redScore) winTeam = Team.Blue;
        else if (logs[checkTurn].serverGameState.blueScore > logs[checkTurn].serverGameState.redScore) winTeam = Team.Red;
      }
      resultBlueTeamText.GetComponent<Outline>().effectColor = new Color(18f/255f, 151f/255f, 254f/255f);
      resultRedTeamText.GetComponent<Outline>().effectColor = new Color(218f/255f, 0f/255f, 52f/255f);
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
}