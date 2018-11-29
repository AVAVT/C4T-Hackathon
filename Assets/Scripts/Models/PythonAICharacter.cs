
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting;
using UnityEngine;

public class PythonAICharacter : ICharacterController
{
  private bool isTimedOut = false;
  private bool isCrashed = false;
  private ScriptEngine engine;
  private ScriptSource source;
  private ScriptScope scope;

  public Character Character { get; set; }

  public bool IsTimedOut
  {
    get
    {
      return isTimedOut;
    }
  }

  public bool IsCrashed
  {
    get
    {
      return isCrashed;
    }
  }
  public PythonAICharacter(ScriptEngine engine, ScriptSource source, ScriptScope scope)
  {
    this.engine = engine;
    this.source = source;
    this.scope = scope;
  }

  public string GetClassnameByIndex(int index)
  {
    if (index == 0 || index == 3) return "Planter";
    else if (index == 1 || index == 4) return "Harvester";
    else return "Worm";
  }

  public void DoStart(GameState gameState)
  {
    UnityEngine.Debug.Log($"{Character.characterRole.ToString()}: Do start!");

    var myClass = GetObject(Character.characterRole.ToString());
    string json = JsonUtility.ToJson(gameState);
    UnityEngine.Debug.Log(GetAIResponse(myClass, "do_start", new object[] { json }));
  }

  public async Task<string> DoTurn(GameState gameState)
  {
    UnityEngine.Debug.Log($"{Character.characterRole.ToString()}: Do turn!");

    var ct = new CancellationTokenSource(100);
    var tcs = new TaskCompletionSource<bool>();
    ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
    var myClass = GetObject(Character.characterRole.ToString());

    string result = Directions.STAY;

    Stopwatch sw = Stopwatch.StartNew();
    await Task<string>.Factory.StartNew(() => GetAIResponse(myClass, "do_turn", null), ct.Token).ContinueWith((task) =>
    {
      sw.Stop();
      UnityEngine.Debug.Log(sw.ElapsedMilliseconds);

      if (task.IsFaulted)
      {
        UnityEngine.Debug.Log("Time out!");
        isTimedOut = true;        
      }
      else if (task.IsCanceled || ct.IsCancellationRequested)
      {
        UnityEngine.Debug.Log("Time out!");
        isTimedOut = true;
        //TODO: Time out action -> lose turn
      }
      else
      {
        ct.Cancel();
        result = task.Result;
        UnityEngine.Debug.Log("Task completed!!!");
        UnityEngine.Debug.Log($"Result: {result}");
      }
    });
    return result;
  }

  System.Object GetObject(string className)
  {
    source.Execute(scope);
    return engine.Operations.Invoke(scope.GetVariable(className));
  }

  private string GetAIResponse(System.Object myclass, string methodName, object[] parameters)
  {
    try
    {
      return engine.Operations.InvokeMember(myclass, methodName, parameters);
    }
    catch (System.Exception ex)
    {
      UnityEngine.Debug.Log(ex);
    }
    return null;
  }
}