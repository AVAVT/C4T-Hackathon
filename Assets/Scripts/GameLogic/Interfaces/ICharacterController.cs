using System.Threading.Tasks;

public interface ICharacterController
{
  Task DoStart(GameState gameState, GameConfig gameRule);
  Task<string> DoTurn(GameState gameState, GameConfig gameRule);
  Character Character { get; set; }
  bool IsTimedOut { get; }
  bool IsCrashed { get; }
}