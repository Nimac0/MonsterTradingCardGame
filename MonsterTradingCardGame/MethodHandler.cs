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
        public string HandleMethod(string method, string destination, string body, string authToken)
        {
            UserHandler userHandler = new UserHandler();
            SessionHandler sessionHandler = new SessionHandler();
            CardHandler cardHandler = new CardHandler();
            TradeHandler tradeHandler = new TradeHandler();

            string response= "";
            switch (destination)
            {
                case "/users" when method == "POST":
                    response = userHandler.CreateOrUpdateUser("",body, authToken, true);
                    break;
                case string s when s.StartsWith("/users/"):
                    string username = s.Substring(7);
                    if (method == "GET") response = userHandler.GetUserData(username, authToken, true);
                    if (method == "PUT") response = userHandler.CreateOrUpdateUser(username, body, authToken, false); ;
                    break;
                case "/sessions" when method == "POST":
                    response = sessionHandler.LoginUser(body);
                    break;
                case "/packages" when method == "POST":
                    response = cardHandler.CreateCardPackage(body);
                    break;
                case "/transactions/packages" when method == "POST":
                    response = cardHandler.BuyCardPackage(body);
                    break;
                case "/cards" when method == "GET":
                    response = cardHandler.GetCards(body);
                    break;
                case "/deck":
                    if (method == "GET") response = cardHandler.GetDeck(body);
                    if (method == "PUT") response = cardHandler.CreateDeck(body);
                    break;
                case "/stats" when method == "GET":
                    
                    break;
                case "/scoreboard" when method == "GET":

                    break;
                case "/battles" when method == "POST":
                    //TODO make it so two users can be added to fight constructor
                    break;
                case "/tradings":
                    if (method == "GET") response = tradeHandler.GetTrades();
                    if (method == "POST") response = tradeHandler.PostTrade(body);
                    break;
                case string s when s.StartsWith("/tradings/"):

                    string tradeId = s.Substring(10);
                    if (method == "DELETE") response = tradeHandler.DeleteTrade(tradeId);
                    if (method == "POST") response = tradeHandler.StartTrade(tradeId);
                    break;
                default:
                    break;
            }
            return response;
        }

        
    }
}
