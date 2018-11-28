using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
  public Team myTeam;
  public List<Character> allies = new List<Character>();
  public List<Character> enemies = new List<Character>();
  public List<List<Tile>> map = new List<List<Tile>>();
}
