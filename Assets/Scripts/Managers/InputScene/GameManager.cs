using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public LogicManager logicManager;
	public PythonManager pythonManager;
	public InputUI uIManager;

	void Awake()
	{
		BindManagers();
	}

	void BindManagers()
	{
		logicManager.pythonManager = this.pythonManager;
		logicManager.uiManager = this.uIManager;
		pythonManager.textOutputManager = this.uIManager;
	}
}
