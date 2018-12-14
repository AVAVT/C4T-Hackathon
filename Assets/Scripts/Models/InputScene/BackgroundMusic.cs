using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
  void Awake()
  {
    GameObject[] objs = GameObject.FindGameObjectsWithTag("BackgroundMusic");

    if (objs.Length > 1)
    {
      Destroy(this.gameObject);
    }

    DontDestroyOnLoad(this.gameObject);
  }
}