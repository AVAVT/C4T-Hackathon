using System;

[System.Serializable]
public struct Character
{
  public int x;
  public int y;
  public Team team;
  public CharacterRole characterRole;
  public int harvest;

  public Character(int x, int y, Team team, CharacterRole characterRole)
  {
    this.characterRole = characterRole;
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

public enum CharacterRole : int
{
  Planter = 0,
  Harvester = 1,
  Worm = 2
}