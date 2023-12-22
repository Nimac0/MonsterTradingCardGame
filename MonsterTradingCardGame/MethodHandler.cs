using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{

    internal class MethodHandler
    {
        IDbHandler dbHandler;
        ISessionHandler sessionHandler;
        ICardHandler cardHandler;
        ITradeHandler tradeHandler;

        public void HandleMethod(string method, string destination, string body)
        {
            switch (destination)
            {
                case "/users" when method == "POST":
                    dbHandler.RegisterUser(body);
                    break;
                case string s when s.StartsWith("/users/"):
                    string username = s.Substring(7);
                    if (method == "GET") dbHandler.GetUserData(username);
                    if (method == "PUT") dbHandler.UpdateUserData(username);
                    break;
                case "/sessions" when method == "POST":
                    sessionHandler.LoginUser(body);
                    break;
                case "/packages" when method == "POST":
                    cardHandler.CreateCardPackage(body);
                    break;
                case "/transactions/packages" when method == "POST":
                    cardHandler.CreateCardPackage(body);
                    break;
                case "/cards" when method == "GET":
                    cardHandler.CreateCardPackage(body);
                    break;
                case "/deck":
                    if (method == "GET") cardHandler.GetDeck(body);
                    if (method == "PUT") cardHandler.CreateDeck(body);
                    break;
                case "/stats" when method == "GET":
                    
                    break;
                case "/scoreboard" when method == "GET":

                    break;
                case "/battles" when method == "POST":
                    //TODO make it so two users can be added to fight constructor
                    break;
                case "/tradings":
                    if (method == "GET") tradeHandler.GetTrades();
                    if (method == "POST") tradeHandler.PostTrades(body);
                    break;
                case string s when s.StartsWith("/tradings/"):

                    string tradeId = s.Substring(10);
                    if (method == "DELETE") tradeHandler.DeleteTrade(tradeId);
                    if (method == "POST") tradeHandler.StartTrade(tradeId);
                    break;
                default:
                    break;
            }
        }

        public bool ValidRequest { get; set; }

        public MethodHandler()
        {

        }
    }
}
