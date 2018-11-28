using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPythonInterpreter {
	string GetResult(string fileName,string className, string methodName);
}
