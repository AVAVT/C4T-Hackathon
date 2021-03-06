using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[JsonObject]
[System.Serializable]
public class TeamRoleMap<T> : IEnumerable<T>, ISerializable
{
  [JsonProperty(TypeNameHandling=TypeNameHandling.None)]
  private Dictionary<Team, Dictionary<CharacterRole, T>> map;
  private IEnumerable<T> flattenedMap;
  public TeamRoleMap()
  {
    map = new Dictionary<Team, Dictionary<CharacterRole, T>>();
    flattenedMap = new List<T>();
  }

  public T GetItem(Team team, CharacterRole role)
  {
    return map[team][role];
  }

  public Dictionary<CharacterRole, T> GetItemsBy(Team team)
  {
    Dictionary<CharacterRole, T> result = new Dictionary<CharacterRole, T>();
    if (map.ContainsKey(team))
    {
      foreach (var kvp in map[team])
      {
        result[kvp.Key] = kvp.Value;
      }
    }

    return result;
  }

  public IEnumerable<T> GetItemsBy(CharacterRole role)
  {
    List<T> result = new List<T>();
    foreach (var kvp in map)
    {
      if (kvp.Value.ContainsKey(role)) result.Add(kvp.Value[role]);
    }

    return result;
  }

  public IEnumerable<Team> GetTeams()
  {
    return map.Keys;
  }

  public void SetItem(Team team, CharacterRole role, T item)
  {
    if (!map.ContainsKey(team)) map[team] = new Dictionary<CharacterRole, T>();
    map[team][role] = item;

    UpdateFlattenedList();
  }

  public bool DeleteItem(Team team, CharacterRole role)
  {
    if (map.ContainsKey(team) && map[team].ContainsKey(role))
    {
      var result = map[team].Remove(role);
      UpdateFlattenedList();
      return result;
    }
    else return false;
  }

  public void ReplaceWithItemFrom(TeamRoleMap<T> other, Team team, CharacterRole role)
  {
    SetItem(team, role, other.GetItem(team, role));
  }

  void UpdateFlattenedList()
  {
    flattenedMap = map.SelectMany(d => d.Value).Select(kvp => kvp.Value);
  }

  public IEnumerator<T> GetEnumerator()
  {
    return flattenedMap.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return flattenedMap.GetEnumerator();
  }

  public void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    info.AddValue("map", map, map.GetType());
    info.AddValue("flattenedMap", flattenedMap);
  }

  public TeamRoleMap(SerializationInfo info, StreamingContext context)
  {
    map = (Dictionary<Team, Dictionary<CharacterRole, T>>) info.GetValue("map", map.GetType());
    flattenedMap = (List<T>) info.GetValue("flattenedMap", flattenedMap.GetType());    
  }
}