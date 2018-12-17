using System.Threading.Tasks;

public interface ICharacterController
{
  Task DoStart(GameState gameState, GameRule gameRule);
  Task<string> DoTurn(GameState gameState, GameRule gameRule);
  Character Character { get; set; }
  bool IsTimedOut { get; }
  bool IsCrashed { get; }
}