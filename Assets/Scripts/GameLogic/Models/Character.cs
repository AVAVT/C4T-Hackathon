using System;
using System.Numerics;

[System.Serializable]
public struct Character
{
  public int x;
  public int y;
  public Team team;
  public CharacterRole characterRole;
  public int fruitCarrying;
  public bool isScared;

  public int numTreePlanted;
  public int numWormCaught;
  public int numFruitHarvested;
  public int numFruitDelivered;
  public int numTreeDestroyed;
  public int numHarvesterScared;

  public Character(int x, int y, Team team, CharacterRole characterRole)
  {
    this.characterRole = characterRole;
    this.x = x;
    this.y = y;
    this.team = team;
    this.fruitCarrying = 0;
    this.isScared = false;

    // TODO subclass to record only role-specific action
    numTreePlanted = numWormCaught = numFruitHarvested = numFruitDelivered = numTreeDestroyed = numHarvesterScared = 0;
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

[System.Serializable]
public enum CharacterRole : int
{
  Planter = 0,
  Harvester = 1,
  Worm = 2
}