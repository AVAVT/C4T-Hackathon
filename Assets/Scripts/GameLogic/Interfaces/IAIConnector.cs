public interface ICharacterController
{
  void DoStart(GameState gameState);
  string DoTurn(GameState gameState);
  Character Character { get; set; }
  bool IsTimedOut { get; }
  bool IsCrashed { get; }
}