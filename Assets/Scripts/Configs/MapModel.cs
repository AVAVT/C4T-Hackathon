using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "_MapModel", menuName = "Configs/MapModel", order = 1)]
public class MapModel:ScriptableObject
{
  public List<MapConfig> listMap;
}