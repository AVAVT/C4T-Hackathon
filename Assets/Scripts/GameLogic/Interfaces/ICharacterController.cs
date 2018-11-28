using System.Threading.Tasks;

public interface ICharacterController
{
  void DoStart(GameState gameState);
  Task<string> DoTurn(GameState gameState);
  Character Character { get; set; }
  bool IsTimedOut { get; }
  bool IsCrashed { get; }
}