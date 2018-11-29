using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPythonInterpreter {
	void InitEngine();
	bool IsHavingAllFiles();
	string GetResult(string fileName,string className, string methodName);
	void WaitAIResponse(int i, string className, string methodName);
}
