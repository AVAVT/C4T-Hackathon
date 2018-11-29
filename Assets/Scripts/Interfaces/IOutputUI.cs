using System;

public interface IOutputUI
{
  void ShowOutputText(string text);
  void ShowPathByIndex(int index, string path);
}