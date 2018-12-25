using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlashScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine(LoadFirstScene());
	}
	
	IEnumerator LoadFirstScene()
	{
		yield return new WaitForSeconds(0.1f);
		SceneManager.LoadScene("InputScene");
	}
}
