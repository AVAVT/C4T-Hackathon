using System;
using System.IO;
using Crosstales.FB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour
{
  public Action<string, float, float> ShowNotiPanel { private get; set; }
  public Action<int, bool> SetIsBot { private get; set; }
  public Action<bool> ChangeMap { private get; set; }
  public Action StartGame { private get; set; }
  public Action EnableLoadingPanel { private get; set; }
  public Action EnableStartPanel { private get; set; }


  [Header("Team panel")]
  [SerializeField] private InputField[] characterInputNames;
  [SerializeField] private Image[] characterStatus;
  [SerializeField] private Button[] characterBrowseButtons;
  [SerializeField] private Button[] characterRemoveButtons;
  [SerializeField] private Sprite unreadyStatusSprite;
  [SerializeField] private Sprite readyStatusSprite;

  [Header("Map info")]
  [SerializeField] private Image mapPreview;
  [SerializeField] private TMP_Text mapName;

  private void SavePlayerName()
  {
    for (int i = 0; i < characterInputNames.Length; i++)
    {
      if (!String.IsNullOrEmpty(characterInputNames[i].text))
        PlayerPrefs.SetString($"Player{i}Name", characterInputNames[i].text);
      else
        PlayerPrefs.SetString($"Player{i}Name", $"Player {i + 1}");
    }
  }

  private bool isFolderContainPython(string path)
  {
    foreach (var dir in Directory.GetFiles(path))
    {
      if (Path.GetFileName(dir).Equals("main.py")) return true;
    }
    return false;
  }

  private void OpenFileBrowser(int index)
  {
    string path = FileBrowser.OpenSingleFolder("Choose AI folder");
    if (!String.IsNullOrEmpty(path) && isFolderContainPython(path))
    {
      var copyPath = GetPathByIndex(index, path);
      EmptyDirectory(copyPath);
      CopyAllDirectory(path, copyPath);
      ShowNotiPanel?.Invoke($"Import folder containing main.py successfully!", 2, 1);
      ShowFileStatus(index);
      SetIsBot?.Invoke(index, true);
    }
    else
    {
      ShowNotiPanel?.Invoke("Invalid path given or folder does not contain main.py file!", 2, 1);
    }
  }

  private void RemoveAIDirectory(int index)
  {
    characterRemoveButtons[index].gameObject.SetActive(false);
    characterBrowseButtons[index].gameObject.SetActive(true);
    characterStatus[index].sprite = unreadyStatusSprite;
    SetIsBot?.Invoke(index, false);
  }

  private void CopyAllDirectory(string sourceDir, string targetDir)
  {
    foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
      Directory.CreateDirectory(Path.Combine(targetDir, dir.Substring(sourceDir.Length + 1)));
    foreach (var fileName in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
      File.Copy(fileName, Path.Combine(targetDir, fileName.Substring(sourceDir.Length + 1)), true);
  }

  private void EmptyDirectory(string path)
  {
    if (Directory.Exists(path))
    {
      Directory.Delete(path, true);
    }
    Directory.CreateDirectory(path);
  }

  private string GetPathByIndex(int index, string path)
  {
    string tempPath;
    switch (index)
    {
      case 0:
        tempPath = $"{GrpcInputManager.DATAPATH}/red_planter";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 1:
        tempPath = $"{GrpcInputManager.DATAPATH}/red_harvester";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 2:
        tempPath = $"{GrpcInputManager.DATAPATH}/red_worm";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 3:
        tempPath = $"{GrpcInputManager.DATAPATH}/blue_planter";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 4:
        tempPath = $"{GrpcInputManager.DATAPATH}/blue_harvester";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      case 5:
        tempPath = $"{GrpcInputManager.DATAPATH}/blue_worm";
        Directory.CreateDirectory(tempPath);
        return tempPath;
      default:
        throw new System.Exception("Index of out bound!!!");
    }
  }

  private void ShowFileStatus(int index)
  {
    characterBrowseButtons[index].gameObject.SetActive(false);
    characterRemoveButtons[index].gameObject.SetActive(true);
    characterStatus[index].sprite = readyStatusSprite;
  }

  public void ShowMapInfo(MapDisplayData mapInfo)
  {
    mapName.text = $"{mapInfo.mapName}";
    mapPreview.sprite = mapInfo.mapPreview;
  }

  //-------------------------------------- Button methods ----------------------------------
  public void OnNextMapButtonClick()
  {
    ChangeMap?.Invoke(true);
  }
  public void OnPrevMapButtonClick()
  {
    ChangeMap?.Invoke(false);
  }
  public void OnLoadAIFolderButtonClick(int index)
  {
    OpenFileBrowser(index);
  }

  public void OnRemoveButtonClick(int index)
  {
    RemoveAIDirectory(index);
  }

  public void OnPlayButtonClick()
  {
    SavePlayerName();
    EnableLoadingPanel?.Invoke();
    StartGame?.Invoke();
  }

  public void BackFromInputPanel()
  {
    EnableStartPanel?.Invoke();
  }
}
