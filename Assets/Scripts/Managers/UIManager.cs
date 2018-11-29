using System;
using UnityEngine;
using UnityEngine.UI;
public class UIManager: MonoBehaviour,IOutputUI
{
  public InputField farmer1Input,collector1Input,worm1Input,farmer2Input,collector2Input,worm2Input;
  public Text farmer1Path,collector1Path,worm1Path,farmer2Path,collector2Path,worm2Path;
  public Text outputText;

  public void SavePlayerName()
  {
    if(!String.IsNullOrEmpty(farmer1Input.text)) PlayerPrefs.SetString("blue-planter", farmer1Input.text); 
    else PlayerPrefs.SetString("blue-planter", farmer1Input.text);
    if(!String.IsNullOrEmpty(collector1Input.text)) PlayerPrefs.SetString("blue-harvester", collector1Input.text); 
    else PlayerPrefs.SetString("blue-harvester", collector1Input.text);
    if(!String.IsNullOrEmpty(worm1Input.text)) PlayerPrefs.SetString("blue-worm", worm1Input.text); 
    else PlayerPrefs.SetString("blue-worm", worm1Input.text);
    if(!String.IsNullOrEmpty(farmer2Input.text)) PlayerPrefs.SetString("red-planter", farmer2Input.text); 
    else PlayerPrefs.SetString("red-planter", farmer2Input.text);
    if(!String.IsNullOrEmpty(collector2Input.text)) PlayerPrefs.SetString("red-harvester", collector2Input.text); 
    else PlayerPrefs.SetString("red-harvester", collector2Input.text);
    if(!String.IsNullOrEmpty(worm2Input.text)) PlayerPrefs.SetString("red-worm", worm2Input.text); 
    else PlayerPrefs.SetString("red-worm", worm2Input.text);
  }

  public void ShowOutputText(string text)
  {
    outputText.text += text + "\n";
  }

  public void ShowPathByIndex(int index, string path)
  {
    switch(index)
    {
      case 0:
        farmer1Path.text = path;
      break;
      case 1:
        collector1Path.text = path;
      break;
      case 2:
        worm1Path.text = path;
      break;
      case 3:
        farmer2Path.text = path;
      break;
      case 4:
        collector2Path.text = path;
      break;
      case 5:
        worm2Path.text = path;
      break;
      default:
        throw new System.Exception("Invalid index or path!");
    }
  }
}