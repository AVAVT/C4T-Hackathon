using UnityEngine;

public struct Character
{
  public Vector2Int position;
  public Team team;
  public CharacterClass characterClass;
  public int harvest;

  public Character(Vector2Int position, Team team, CharacterClass characterClass)
  {
    this.characterClass = characterClass;
    this.position = position;
    this.team = team;
    this.harvest = 0;
  }
}

public enum CharacterClass : int
{
  Planter = 0,
  Harvester = 1,
  Worm = 2
}