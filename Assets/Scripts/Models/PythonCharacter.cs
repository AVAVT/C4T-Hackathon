
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using UnityEngine;

public class PythonCharacter : ICharacterController
{
  public IInputSceneUI uiManager;
  private Channel channel;
  private AIService.AIServiceClient client;
  private bool isTimedOut = false;
  private bool isCrashed = false;

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

  public PythonCharacter(Channel channel)
  {
    this.channel = channel;
    this.client = new AIService.AIServiceClient(channel);
  }

  public void DoStart(GameState gameState)
  {
    string json = JsonConvert.SerializeObject(gameState);
    try
    {
      AIResponse reply = client.ReturnAIResponse(new AIRequest { Json = json });
    }
    catch (System.Exception ex)
    {
      uiManager.SaveErrorMessage($"Get AI Response fail! Fail message: {ex}", true);
    }
  }

  public async Task<string> DoTurn(GameState gameState)
  {
    UpdateCharacter(gameState);

    string result = Directions.STAY;
    if (!isCrashed && !IsTimedOut)
    {
      var ct = new CancellationTokenSource(100);
      var tcs = new TaskCompletionSource<bool>();
      ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

      string json = JsonConvert.SerializeObject(gameState);
     
      await Task<string>.Factory.StartNew(() => GetAIResponse(json), ct.Token).ContinueWith((task) =>
      {
        if (task.IsFaulted && !isValidAction(result))
        {
          uiManager.SaveErrorMessage($"Character: {Character.characterRole} - Team: {Character.team} is crashed! Error: {task.Exception}", true);
          isCrashed = true;
        }
        else if (task.IsCanceled || ct.IsCancellationRequested)
        {
          uiManager.SaveErrorMessage($"Character: {Character.characterRole} - Team: {Character.team} is timeout!",true);
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

  private string GetAIResponse(string json)
  {
    try
    {
      AIResponse reply = client.ReturnAIResponse(new AIRequest { Index = (int)Character.team * 3 + (int)Character.characterRole, Json = json });
      return reply.Action;
    }
    catch (System.Exception ex)
    {
      uiManager.SaveErrorMessage($"Get AI Response fail! Fail message: {ex}", true);
      UnityEngine.Debug.LogError($"Fail! Message: {ex}");
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