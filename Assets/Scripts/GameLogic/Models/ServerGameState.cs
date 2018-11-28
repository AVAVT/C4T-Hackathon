using System.Collections.Generic;

public class ServerGameState
{
  public List<Character> teamRed = new List<Character>();
  public List<Character> teamBlue = new List<Character>();
  public List<List<Tile>> map = new List<List<Tile>>();
}