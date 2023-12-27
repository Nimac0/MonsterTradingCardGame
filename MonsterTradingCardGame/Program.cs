using MonsterTradingCardGame;

namespace MonsterTradingCardGame
{
    public abstract class Program
    {
        static void Main(string[] args)
        {
            ConnectionHandler connectionHandler = new ConnectionHandler();
            connectionHandler.HandleConnection();
        }
    }
}
