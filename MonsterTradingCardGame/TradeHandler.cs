using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class TradeHandler
    {
        public string GetTrades(string authToken)
        {
            string? authorizedUser = SessionHandler.GetUsernameByToken(authToken);
            if (string.IsNullOrEmpty(authorizedUser)) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            DbHandler dbHandler = new DbHandler(@"SELECT * FROM trades;");
            List<Trade> trades = new List<Trade>();
            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                while (reader.Read())
                {
                    Trade newTrade = new Trade()
                    {
                        Id = reader.GetString(0),
                        RequiredType = reader.GetString(1),
                        RequiredDamage = reader.GetFloat(2),
                        CardId = reader.GetString(3)
                    };
                    trades.Add(newTrade);
                }
                if (trades.Count != 0) return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(trades), "application/json");
            }
            return Response.CreateResponse("204", "No Content", "", "application/json");
        }

        public string PostTrade(string authToken, string requestBody)
        {
            Trade newTrade = new Trade();
            try
            {
                newTrade = JsonConvert.DeserializeObject<Trade>(requestBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            int? userId = SessionHandler.GetIdByUsername(SessionHandler.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            DbHandler checkTradeId = new DbHandler(@"SELECT * FROM trades WHERE id = @tradeid");
            checkTradeId.AddParameterWithValue("tradeid", DbType.String, newTrade.Id);

            using (IDataReader reader = checkTradeId.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (reader.GetString(0) != null) return Response.CreateResponse("409", "Conflict", "", "application/json");
                }
            }

            if (!this.CheckIfCardOwner(newTrade.CardId, userId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            DbHandler createTrade = new DbHandler(@"INSERT INTO trades (id, requiredtype, requireddamage, cardid) VALUES (@id, @requiredtype, @requireddamage, @cardid) RETURNING id;");
            createTrade.AddParameterWithValue("id", DbType.String, newTrade.Id);
            createTrade.AddParameterWithValue("requiredtype", DbType.String, newTrade.RequiredType);
            createTrade.AddParameterWithValue("requireddamage", DbType.Decimal, newTrade.RequiredDamage);
            createTrade.AddParameterWithValue("cardid", DbType.String, newTrade.CardId);
            // TODO switch intrade to true
            string id = (string)(createTrade.ExecuteScalar() ?? "");
            if (id != "") return Response.CreateResponse("201", "Created", "", "application/json");
            return "???";
        }

        public string DeleteTrade(string tradeId, string authToken)
        {
            int? userId = SessionHandler.GetIdByUsername(SessionHandler.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            DbHandler getTradeFromId = new DbHandler(@"SELECT * FROM trades WHERE id = @id;");
            Trade currTrade = new Trade();
            using (IDataReader reader = getTradeFromId.ExecuteReader())
            {
                if (reader.Read())
                {
                    currTrade.Id = reader.GetString(0);
                    currTrade.RequiredType = reader.GetString(1);
                    currTrade.RequiredDamage = reader.GetFloat(2);
                    currTrade.CardId = reader.GetString(3);
                }
                if(currTrade.Id == null) return Response.CreateResponse("404", "Not Found", "", "application/json");
            }

            if (!this.CheckIfCardOwner(currTrade.CardId, userId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");


            return "trade deleted";
        }

        public string StartTrade(string tradeId)
        {
            return "trade started";
        }

        public bool CheckIfCardOwner(string cardId, int? userId)
        {
            DbHandler checkOwner = new DbHandler(@"SELECT * FROM cards WHERE id = @cardid AND userid = @userid AND indeck = FALSE;");
            checkOwner.AddParameterWithValue("cardid", DbType.String, cardId);
            checkOwner.AddParameterWithValue("userid", DbType.Int32, userId);

            using (IDataReader reader = checkOwner.ExecuteReader())
            {
                if (!reader.Read()) return false;
            }
            return true;
        }
    }
}
