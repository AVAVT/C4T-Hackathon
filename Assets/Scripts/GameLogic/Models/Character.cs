using System;

public struct Character
{
  public int x;
  public int y;
  public Team team;
  public CharacterClass characterClass;
  public int harvest;

  public Character(int x, int y, Team team, CharacterClass characterClass)
  {
    this.characterClass = characterClass;
    this.x = x;
    this.y = y;
    this.team = team;
    this.harvest = 0;
  }

  public int DistanceTo(Character character)
  {
    return Math.Abs(x - character.x) + Math.Abs(y - character.y);
  }

  public int DistanceTo(Tile tile)
  {
    return Math.Abs(x - tile.x) + Math.Abs(y - tile.y);
  }
}

public enum CharacterClass : int
{
  Planter = 0,
  Harvester = 1,
  Worm = 2
}