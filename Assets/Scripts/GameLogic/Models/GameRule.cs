using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class GameRule
{
  public readonly int sightDistance;
  public readonly int gameLength;
  public readonly int plantFruitTime;
  public readonly int wildberryFruitTime;
  public readonly Dictionary<TileType, int> harvestValue;
  public readonly List<Team> availableTeams;
  public readonly List<CharacterRole> availableRoles;

  public static GameRule DefaultGameRule()
  {
    return new GameRule(
      sightDistance: 2,
      gameLength: 100,
      plantFruitTime: 5,
      wildberryFruitTime: 10,
      fruitHarvestValue: 1,
      wildberryHarvestValue: 2,
      availableTeams: new List<Team>() { Team.Red, Team.Blue },
      availableRoles: new List<CharacterRole>() { CharacterRole.Planter, CharacterRole.Harvester, CharacterRole.Worm }
    );
  }

  public GameRule(int sightDistance, int gameLength, int plantFruitTime, int wildberryFruitTime, int fruitHarvestValue, int wildberryHarvestValue, List<Team> availableTeams, List<CharacterRole> availableRoles)
  {
    this.sightDistance = sightDistance;
    this.gameLength = gameLength;
    this.plantFruitTime = plantFruitTime;
    this.wildberryFruitTime = wildberryFruitTime;
    this.harvestValue = new Dictionary<TileType, int>(){
      {TileType.TOMATO, fruitHarvestValue},
      {TileType.PUMPKIN, fruitHarvestValue},
      {TileType.WILDBERRY, wildberryHarvestValue},
    };
    this.availableTeams = availableTeams;
    this.availableRoles = availableRoles;
  }
}