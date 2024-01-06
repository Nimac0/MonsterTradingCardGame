using MonsterTradingCardGame.http;

namespace MonsterTradingCardGame
{
    public abstract class Program
    {
        static void Main(string[] args)
        {
            Connection connectionHandler = new Connection();
            connectionHandler.HandleConnection();
        }
    }
}
