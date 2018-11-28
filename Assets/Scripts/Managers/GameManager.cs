using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public LogicManager logicManager;
	public PythonManager pythonManager;

	void Awake()
	{
		BindManagers();
	}

	void BindManagers()
	{
		logicManager.pythonManager = this.pythonManager;
	}
}
