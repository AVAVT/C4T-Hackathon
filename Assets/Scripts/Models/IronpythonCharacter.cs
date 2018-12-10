
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting;
using UnityEngine;

public class IronpythonCharacter : ICharacterController
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
  public IronpythonCharacter(ScriptEngine engine, ScriptSource source, ScriptScope scope)
  {
    this.engine = engine;
    this.source = source;
    this.scope = scope;
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
    UpdateCharacter(gameState);

    string result = Directions.STAY;
    if (!isCrashed && !IsTimedOut)
    {
      var ct = new CancellationTokenSource(100);
      var tcs = new TaskCompletionSource<bool>();
      ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
      var myClass = GetObject(Character.characterRole.ToString());

      string json = JsonUtility.ToJson(gameState);
      Stopwatch sw = Stopwatch.StartNew();
      await Task<string>.Factory.StartNew(() => GetAIResponse(myClass, "do_turn", new object[] { json }), ct.Token).ContinueWith((task) =>
      {
        sw.Stop();
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);

        if (task.IsFaulted && !isValidAction(result))
        {
          UnityEngine.Debug.Log($"AI is crashed! Error: {task.Exception}");
          isCrashed = true;
        }
        else if (task.IsCanceled || ct.IsCancellationRequested)
        {
          UnityEngine.Debug.Log("Time out!");
          isTimedOut = true;
        }
        else
        {
          ct.Cancel();
          result = task.Result;
          UnityEngine.Debug.Log("Task completed!!!");
          UnityEngine.Debug.Log($"Result: {result}");
        }
      });
    }
    return result;
  }

  bool isValidAction(string action)
  {
    if (action.ToUpper() == Directions.DOWN) return true;
    if (action.ToUpper() == Directions.UP) return true;
    if (action.ToUpper() == Directions.LEFT) return true;
    if (action.ToUpper() == Directions.RIGHT) return true;
    if (action.ToUpper() == Directions.STAY) return true;
    return false;
  }

  void UpdateCharacter(GameState gameState)
  {
    foreach (var ally in gameState.allies)
    {
      if (ally.characterRole == Character.characterRole)
      {
        Character = ally;
        UnityEngine.Debug.Log($"x: {Character.x} - y: {Character.y}");
      }
    }
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