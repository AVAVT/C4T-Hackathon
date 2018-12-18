using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class GameConfig
{
  public readonly int sightDistance;
  public readonly int gameLength;
  public readonly int plantFruitTime;
  public readonly int wildberryFruitTime;
  public readonly int harvesterMaxCapacity;
  public readonly Dictionary<TileType, int> fruitScoreValues;
  public readonly List<Team> availableTeams;
  public readonly List<CharacterRole> availableRoles;

  public static GameConfig DefaultGameRule()
  {
    return new GameConfig(
      sightDistance: 2,
      gameLength: 100,
      plantFruitTime: 5,
      wildberryFruitTime: 10,
      fruitHarvestValue: 1,
      wildberryHarvestValue: 2,
      harvesterMaxCapacity: 5,
      availableTeams: new List<Team>() { Team.Red, Team.Blue },
      availableRoles: new List<CharacterRole>() { CharacterRole.Planter, CharacterRole.Harvester, CharacterRole.Worm }
    );
  }

  public GameConfig(int sightDistance, int gameLength, int plantFruitTime, int wildberryFruitTime, int fruitHarvestValue, int wildberryHarvestValue, int harvesterMaxCapacity, List<Team> availableTeams, List<CharacterRole> availableRoles)
  {
    this.sightDistance = sightDistance;
    this.gameLength = gameLength;
    this.plantFruitTime = plantFruitTime;
    this.wildberryFruitTime = wildberryFruitTime;
    this.harvesterMaxCapacity = harvesterMaxCapacity;
    this.fruitScoreValues = new Dictionary<TileType, int>(){
      {TileType.TOMATO, fruitHarvestValue},
      {TileType.PUMPKIN, fruitHarvestValue},
      {TileType.WILDBERRY, wildberryHarvestValue},
    };
    this.availableTeams = availableTeams;
    this.availableRoles = availableRoles;
  }

  public TileType ScoreTileTypeForTeam(Team team) => team == Team.Red ? TileType.RED_BOX : TileType.BLUE_BOX;
  public TileType FruitTileTypeForTeam(Team team) => team == Team.Red ? TileType.TOMATO : TileType.PUMPKIN;
  public List<TileType> WormDestroyTileTypeForTeam(Team team) => new List<TileType>() { team == Team.Red ? TileType.PUMPKIN : TileType.TOMATO };
}