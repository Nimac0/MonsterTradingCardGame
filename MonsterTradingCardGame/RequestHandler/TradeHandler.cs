using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.http;
using MonsterTradingCardGame.Schemas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.RequestHandler
{
    public class TradeHandler
    {
        public DbQuery dbQuery = new DbQuery();

        public string GetTrades(string authToken)
        {
            string? authorizedUser = SessionHandler.Instance.GetUsernameByToken(authToken);
            if (string.IsNullOrEmpty(authorizedUser)) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            DbQuery dbHandler = this.dbQuery.NewCommand(@"SELECT * FROM trades;");
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

            int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            if (TradeIdExists(newTrade.CardId)) return Response.CreateResponse("409", "Conflict", "", "application/json");

            if (!IsCardOwner(newTrade.CardId, userId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            DbQuery putInTrade = this.dbQuery.NewCommand(@"UPDATE cards SET intrade = TRUE WHERE id = @cardid;");

            putInTrade.AddParameterWithValue("cardid", DbType.String, newTrade.CardId);
            putInTrade.ExecuteNonQuery();

            DbQuery createTrade = this.dbQuery.NewCommand(@"INSERT INTO trades (id, requiredtype, requireddamage, cardid) VALUES (@id, @requiredtype, @requireddamage, @cardid) RETURNING id;");
            createTrade.AddParameterWithValue("id", DbType.String, newTrade.Id);
            createTrade.AddParameterWithValue("requiredtype", DbType.String, newTrade.RequiredType);
            createTrade.AddParameterWithValue("requireddamage", DbType.Decimal, newTrade.RequiredDamage);
            createTrade.AddParameterWithValue("cardid", DbType.String, newTrade.CardId);

            string id = (string)(createTrade.ExecuteScalar() ?? "");
            if (id != "") return Response.CreateResponse("201", "Created", "", "application/json");

            return "???";
        }

        public string DeleteTrade(string tradeId, string authToken)
        {
            int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            Trade currTrade = GetTradeFromId(tradeId);

            if (!IsCardOwner(currTrade.CardId, userId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            DbQuery putInTrade = this.dbQuery.NewCommand(@"UPDATE cards SET intrade = FALSE WHERE id = @cardid;");
            putInTrade.AddParameterWithValue("cardid", DbType.String, currTrade.CardId);
            putInTrade.ExecuteNonQuery();

            DbQuery deleteTrade = this.dbQuery.NewCommand(@"DELETE FROM trades WHERE id = @tradeid;");
            deleteTrade.AddParameterWithValue("tradeid", DbType.String, tradeId);
            deleteTrade.ExecuteNonQuery();

            return Response.CreateResponse("200", "OK", "", "application/json");
        }

        public string StartTrade(string requestBody, string tradeId, string authToken) //requestBody here should be cardid of offered card
        {
            string cardId = (string)JsonConvert.DeserializeObject(requestBody);
            int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            if (!TradeIdExists(tradeId)) return Response.CreateResponse("404", "Not Found", "", "application/json");

            Trade currTrade = GetTradeFromId(tradeId);
            if (!IsCardOwner(cardId, userId) || !CheckRequirements(currTrade, cardId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            DbQuery getTradeOwner = this.dbQuery.NewCommand(@"SELECT userid FROM cards WHERE id = @cardid;");
            getTradeOwner.AddParameterWithValue("cardid", DbType.String, currTrade.CardId);
            int ownerId = 0;

            using (IDataReader reader = getTradeOwner.ExecuteReader())
            {
                if (reader.Read())
                {
                    ownerId = reader.GetInt32(0);
                }
            }

            if (ownerId == userId) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            PackageHandler packageHandler = new PackageHandler();
            if (!packageHandler.ChangeCardOwner(ownerId, cardId)) return "???";
            if (!packageHandler.ChangeCardOwner(userId, currTrade.CardId)) return "???";

            return Response.CreateResponse("200", "OK", "", "application/json");
        }

        public bool IsCardOwner(string cardId, int? userId)
        {
            DbQuery checkOwner = this.dbQuery.NewCommand(@"SELECT * FROM cards WHERE id = @cardid AND userid = @userid AND indeck = FALSE;");
            checkOwner.AddParameterWithValue("cardid", DbType.String, cardId);
            checkOwner.AddParameterWithValue("userid", DbType.Int32, userId);

            using (IDataReader reader = checkOwner.ExecuteReader())
            {
                if (!reader.Read()) return false;
            }
            return true;
        }

        public bool TradeIdExists(string tradeId)
        {
            DbQuery checkTradeId = this.dbQuery.NewCommand(@"SELECT * FROM trades WHERE id = @tradeid");
            checkTradeId.AddParameterWithValue("tradeid", DbType.String, tradeId);

            using (IDataReader reader = checkTradeId.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (reader.GetString(0) != null) return true;
                }
            }
            return false;
        }

        public Trade GetTradeFromId(string tradeId)
        {
            DbQuery getTradeFromId = this.dbQuery.NewCommand(@"SELECT * FROM trades WHERE id = @id;");
            getTradeFromId.AddParameterWithValue("id", DbType.String, tradeId);

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
                if (currTrade.Id == null) return null;
            }
            return currTrade;
        }

        public bool CheckRequirements(Trade currTrade, string cardId)
        {
            DbQuery getCardStats = this.dbQuery.NewCommand(@"SELECT cardtype, damage, indeck FROM cards WHERE id = @cardid");
            getCardStats.AddParameterWithValue("cardid", DbType.String, cardId);

            CardType type = 0;
            float damage = 0;
            bool indeck = false;

            using (IDataReader reader = getCardStats.ExecuteReader())
            {
                if (reader.Read())
                {
                    type = (CardType)reader.GetInt32(0);
                    damage = reader.GetFloat(1);
                    indeck = reader.GetBoolean(2);
                }
            }
            if (type == CardType.SPELL && currTrade.RequiredType == "monster" ||
                type != CardType.SPELL && currTrade.RequiredType == "spell") return false;
            if (damage < currTrade.RequiredDamage || indeck) return false;

            return true;

        }
    }
}
