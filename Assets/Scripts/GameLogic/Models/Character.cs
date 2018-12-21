using System;
using System.Numerics;

[System.Serializable]
public class Character
{
  public int x;
  public int y;
  public Team team;
  public CharacterRole characterRole;
  public bool isScared;
  public bool cancelAction;
  public int fruitCarrying;
  public int numTreePlanted, numWormCaught, numFruitHarvested, numFruitDelivered, numTreeDestroyed, numHarvesterScared;

  //TODO: Sub-class
  public Character(int x, int y, Team team, CharacterRole characterRole)
  {
    this.characterRole = characterRole;
    this.x = x;
    this.y = y;
    this.team = team;
    this.isScared = false;
    this.cancelAction = false;

    fruitCarrying = numTreePlanted = numWormCaught = numFruitHarvested = numFruitDelivered = numTreeDestroyed = numHarvesterScared;
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