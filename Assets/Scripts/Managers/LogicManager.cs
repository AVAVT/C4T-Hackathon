using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour
{
  public IPythonInterpreter pythonManager;

  [Header("Map")]
  [SerializeField] private Transform gridTransform;
  [SerializeField] private int rows;
  [SerializeField] private int cols;
  [SerializeField] private Vector2 gridSize;
  private Vector2 gridOffset;
  //cells
  [SerializeField] private Sprite cellSprite1, cellSprite2;
  [SerializeField] private Vector2 cellSize;
  private Vector2 cellScale;

  void Start()
  {
    // DrawMap();
  }

  void DrawMap()
  {
    GameObject cellObject = new GameObject();

    Vector2 newCellSize = new Vector2(gridSize.x / (float)cols, gridSize.y / (float)rows);

    cellScale.x = newCellSize.x / cellSize.x;
    cellScale.y = newCellSize.y / cellSize.y;


    cellObject.transform.localScale = new Vector2(cellScale.x, cellScale.y);

    gridOffset.x = -(gridSize.x / 2) + cellSize.x / 2;
    gridOffset.y = -(gridSize.y / 2) + cellSize.y / 2;

    for (int row = 0; row < rows; row++)
    {
      for (int col = 0; col < cols; col++)
      {
        Vector2 pos = new Vector2(col * cellSize.x + gridOffset.x + transform.position.x, row * cellSize.y + gridOffset.y + transform.position.y);
        GameObject cO = Instantiate(cellObject, pos, Quaternion.identity) as GameObject;
        var spriteCO = cO.AddComponent<SpriteRenderer>();

        if (row % 2 == 0)
        {
          if (col % 2 == 0) spriteCO.sprite = cellSprite1;
          else spriteCO.sprite = cellSprite2;
        }
        else
        {
          if (col % 2 == 0) spriteCO.sprite = cellSprite2;
          else spriteCO.sprite = cellSprite1;
        }

        cO.transform.SetParent(gridTransform);
      }
    }
  }

  void OnDrawGizmos()
  {
    Gizmos.DrawWireCube(transform.position, gridSize);
  }

  public enum GameState
  {
    Idle,
    Requesting,
    PerformingAction
  }
}
