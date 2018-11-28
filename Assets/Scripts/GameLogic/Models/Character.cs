using UnityEngine;

public class Character
{
  public Vector2Int position;
  public Team team;
  public CharacterClass characterClass;
  public int harvest = 0;
}

public enum CharacterClass : int
{
  Planter = 0,
  Harvester = 1,
  Worm = 2
}