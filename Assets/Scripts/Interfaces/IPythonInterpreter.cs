using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPythonInterpreter {
	IEnumerator StartRecordGame();
	bool IsHavingAllFiles();
	// void WaitAIResponse(int i, string className, string methodName);
}
