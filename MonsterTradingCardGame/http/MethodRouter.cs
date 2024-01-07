using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.RequestHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.http
{

    public class MethodRouter
    {
        public IDatabase dbQuery = new DbQuery();
        public string HandleMethod(string method, string destination, string body, string authToken)
        {
            UserHandler userHandler = new UserHandler();
            SessionHandler sessionHandler = SessionHandler.Instance;
            CardHandler cardHandler = new CardHandler();
            PackageHandler packageHandler = new PackageHandler();
            TradeHandler tradeHandler = new TradeHandler();
            FightHandler fightHandler = FightHandler.Instance;

            string response = "";

            switch (destination)
            {
                case "/users" when method == "POST":
                    response = userHandler.CreateUser(body, authToken);
                    break;
                case string s when s.StartsWith("/users/"):
                    string username = s.Substring(7);
                    if (method == "GET") response = userHandler.GetUserData(username, authToken, true);
                    if (method == "PUT") response = userHandler.UpdateUser(username, body, authToken); ;
                    break;
                case "/sessions" when method == "POST":
                    response = sessionHandler.LoginUser(body);
                    break;
                case "/packages" when method == "POST":
                    response = packageHandler.CreateCardPackage(body, authToken);
                    break;
                case "/transactions/packages" when method == "POST":
                    response = packageHandler.BuyCardPackage(authToken);
                    break;
                case "/cards" when method == "GET":
                    response = JsonConvert.SerializeObject(cardHandler.GetCardsOrDeck(authToken, false), Formatting.Indented);
                    if (response == "null") return Response.CreateResponse("401", "Unauthorised", "", "application/json");
                    if (response == null) return Response.CreateResponse("204", "No Content", response, "application/json");
                    response = Response.CreateResponse("200", "OK", response, "application/json");
                    break;
                case "/deck":
                    if (method == "GET")
                    {
                        response = JsonConvert.SerializeObject(cardHandler.GetCardsOrDeck(authToken, true), Formatting.Indented);
                        if (response == "null") return Response.CreateResponse("401", "Unauthorised", response, "application/json");
                        if (response == null) return Response.CreateResponse("204", "No Content", response, "application/json");
                        response = Response.CreateResponse("200", "OK", response, "application/json");
                    }
                    if (method == "PUT") response = cardHandler.ChooseDeck(body, authToken);
                    break;
                case "/stats" when method == "GET":
                    response = userHandler.GetUserStats(authToken, false);
                    break;
                case "/scoreboard" when method == "GET":
                    response = userHandler.GetUserStats(authToken, true);
                    break;
                case "/battles" when method == "POST":
                    response = fightHandler.StartLobby(authToken);
                    break;
                case "/tradings":
                    if (method == "GET") response = tradeHandler.GetTrades(authToken);
                    if (method == "POST") response = tradeHandler.PostTrade(authToken, body);
                    break;
                case string s when s.StartsWith("/tradings/"):
                    string tradeId = s.Substring(10);
                    if (method == "DELETE") response = tradeHandler.DeleteTrade(tradeId, authToken);
                    if (method == "POST") response = tradeHandler.StartTrade(body, tradeId, authToken);
                    break;
                default:
                    response = Response.CreateResponse("400", "Bad Request", "", "application/json");
                    break;
            }
            return response;
        }


    }
}
