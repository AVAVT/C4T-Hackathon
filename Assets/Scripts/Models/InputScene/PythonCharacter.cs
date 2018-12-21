
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using UnityEngine;

public class PythonCharacter : ICharacterController, IRuntimeCharacter
{
  public IErrorRecorder errorRecorder;
  private Channel channel;
  private AIService.AIServiceClient client;
  private bool isTimedOut = false;
  private bool isCrashed = false;
  private Action cancelStartGameTask;
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

  public Action CancelStartGameTask
  {
    set
    {
      cancelStartGameTask = value;
    }
  }

  public PythonCharacter(Channel channel)
  {
    this.channel = channel;
    this.client = new AIService.AIServiceClient(channel);
  }

  public async Task DoStart(GameState gameState, GameConfig gameRule)
  {
    string result = "";
    
    if (!isCrashed && !isTimedOut)
    {
      var ct = new CancellationTokenSource(300); //init server need at least 200ms
      var tcs = new TaskCompletionSource<bool>();
      ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

      string json = JsonConvert.SerializeObject(gameState);
      string gameRuleJson = JsonConvert.SerializeObject(gameRule);

      await Task<string>.Factory.StartNew(() => GetAIResponse(json, gameRuleJson), ct.Token).ContinueWith((task) =>
      {
        if (task.IsFaulted)
        {
          UnityEngine.Debug.Log("Crashed!");
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
        }
      });
    }
    if (result != "READY" && !isTimedOut) isCrashed = true;
  }

  public async Task<string> DoTurn(GameState gameState, GameConfig gameRule)
  {
    UpdateCharacter(gameState);

    string result = Directions.STAY;
    if (!isCrashed && !isTimedOut)
    {
      var ct = new CancellationTokenSource(100);
      var tcs = new TaskCompletionSource<bool>();
      ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

      string json = JsonConvert.SerializeObject(gameState);

      await Task<string>.Factory.StartNew(() => GetAIResponse(json), ct.Token).ContinueWith((task) =>
      {
        if (task.IsFaulted && !isValidAction(result))
        {
          isCrashed = true;
        }
        else if (task.IsCanceled || ct.IsCancellationRequested)
        {
          isTimedOut = true;
        }
        else
        {
          ct.Cancel();
          result = task.Result;
        }
      });
    }
    return result;
  }

  private string GetAIResponse(string json, string gameRule = "")
  {
    try
    {
      AIResponse reply = client.ReturnAIResponse(new AIRequest { Index = (int)Character.team * 3 + (int)Character.characterRole, GameRule = gameRule, ServerGameState = json});
      return reply.Action;
    }
    catch (System.Exception ex)
    {
      errorRecorder.RecordErrorMessage($"Get AI Response fail! Fail message: {ex}", true);
      UnityEngine.Debug.LogError($"Fail! Message: {ex}");
      cancelStartGameTask?.Invoke();
      return "STAY";
    }
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
      }
    }
  }
}