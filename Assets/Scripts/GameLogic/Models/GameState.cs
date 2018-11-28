using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
  public int turn;
  public Team myTeam;
  public List<Character> allies = new List<Character>();
  public List<Character> enemies = new List<Character>();
  public List<List<Tile>> map = new List<List<Tile>>();

  public GameState(int turn, Team team)
  {
    this.turn = turn;
    this.myTeam = team;
    allies = new List<Character>();
    enemies = new List<Character>();
    map = new List<List<Tile>>();
  }
}
