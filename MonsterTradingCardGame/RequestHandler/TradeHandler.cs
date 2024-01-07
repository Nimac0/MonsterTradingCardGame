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
        public IDatabase dbQuery = new DbQuery();
        public static Mutex startTradeMutex = new Mutex();

        public string GetTrades(string authToken)
        {
            string? authorizedUser = SessionHandler.Instance.GetUsernameByToken(authToken);
            if (string.IsNullOrEmpty(authorizedUser)) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            IDatabase dbHandler = this.dbQuery.NewCommand(@"SELECT * FROM trades;");
            List<Trade> trades = new List<Trade>();
            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                while (reader.Read())
                {
                    Trade newTrade = new Trade()
                    {
                        Id = reader.GetString(0),
                        Type = reader.GetString(1),
                        MinimumDamage = reader.GetFloat(2),
                        CardToTrade = reader.GetString(3)
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

            if (TradeIdExists(newTrade.CardToTrade)) return Response.CreateResponse("409", "Conflict", "", "application/json");

            if (!IsCardOwner(newTrade.CardToTrade, userId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            IDatabase putInTrade = this.dbQuery.NewCommand(@"UPDATE cards SET intrade = TRUE WHERE id = @cardid;");

            putInTrade.AddParameterWithValue("cardid", DbType.String, newTrade.CardToTrade);
            putInTrade.ExecuteNonQuery();

            IDatabase createTrade = this.dbQuery.NewCommand(@"INSERT INTO trades (id, requiredtype, requireddamage, cardid) VALUES (@id, @requiredtype, @requireddamage, @cardid) RETURNING id;");
            createTrade.AddParameterWithValue("id", DbType.String, newTrade.Id);
            createTrade.AddParameterWithValue("requiredtype", DbType.String, newTrade.Type);
            createTrade.AddParameterWithValue("requireddamage", DbType.Decimal, newTrade.MinimumDamage);
            createTrade.AddParameterWithValue("cardid", DbType.String, newTrade.CardToTrade);

            string id = (string)(createTrade.ExecuteScalar() ?? "");
            if (id != "") return Response.CreateResponse("201", "Created", "", "application/json");

            return "???";
        }

        public string DeleteTrade(string tradeId, string authToken)
        {
            int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            if (!TradeIdExists(tradeId)) return Response.CreateResponse("404", "Not Found", "", "application/json");
            Trade currTrade = GetTradeFromId(tradeId);

            if (!IsCardOwner(currTrade.CardToTrade, userId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            IDatabase takeOutOfTrade = this.dbQuery.NewCommand(@"UPDATE cards SET intrade = FALSE WHERE id = @cardid;");
            takeOutOfTrade.AddParameterWithValue("cardid", DbType.String, currTrade.CardToTrade);
            takeOutOfTrade.ExecuteNonQuery();

            IDatabase deleteTrade = this.dbQuery.NewCommand(@"DELETE FROM trades WHERE id = @tradeid;");
            deleteTrade.AddParameterWithValue("tradeid", DbType.String, tradeId);
            deleteTrade.ExecuteNonQuery();

            return Response.CreateResponse("200", "OK", "", "application/json");
        }

        public string StartTrade(string requestBody, string tradeId, string authToken) //requestBody here should be cardid of offered card
        {
            string cardId = (string)JsonConvert.DeserializeObject(requestBody);
            int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            lock (startTradeMutex)
            {
                if (!TradeIdExists(tradeId)) return Response.CreateResponse("404", "Not Found", "", "application/json");

                Trade currTrade = GetTradeFromId(tradeId);
                if (!IsCardOwner(cardId, userId) || !CheckRequirements(currTrade, cardId)) return Response.CreateResponse("403", "Forbidden", "", "application/json");

                IDatabase getTradeOwner = this.dbQuery.NewCommand(@"SELECT userid FROM cards WHERE id = @cardid;");
                getTradeOwner.AddParameterWithValue("cardid", DbType.String, currTrade.CardToTrade);
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
                if (!packageHandler.ChangeCardOwner(userId, currTrade.CardToTrade)) return "???";

                IDatabase takeOutOfTrade = this.dbQuery.NewCommand(@"UPDATE cards SET intrade = FALSE WHERE id = @cardid;");
                takeOutOfTrade.AddParameterWithValue("cardid", DbType.String, currTrade.CardToTrade);
                takeOutOfTrade.ExecuteNonQuery();

                IDatabase deleteTrade = this.dbQuery.NewCommand(@"DELETE FROM trades WHERE id = @tradeid;");
                deleteTrade.AddParameterWithValue("tradeid", DbType.String, tradeId);
                deleteTrade.ExecuteNonQuery();
            }
            return Response.CreateResponse("200", "OK", "", "application/json");
        }

        public virtual bool IsCardOwner(string cardId, int? userId)
        {
            IDatabase checkOwner = this.dbQuery.NewCommand(@"SELECT * FROM cards WHERE id = @cardid AND userid = @userid AND indeck = FALSE;");
            checkOwner.AddParameterWithValue("cardid", DbType.String, cardId);
            checkOwner.AddParameterWithValue("userid", DbType.Int32, userId);

            using (IDataReader reader = checkOwner.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public virtual bool TradeIdExists(string tradeId)
        {
            IDatabase checkTradeId = this.dbQuery.NewCommand(@"SELECT * FROM trades WHERE id = @tradeid");
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

        public virtual Trade GetTradeFromId(string tradeId)
        {
            IDatabase getTradeFromId = this.dbQuery.NewCommand(@"SELECT * FROM trades WHERE id = @id;");
            getTradeFromId.AddParameterWithValue("id", DbType.String, tradeId);

            Trade currTrade = new Trade();
            using (IDataReader reader = getTradeFromId.ExecuteReader())
            {
                if (reader.Read())
                {
                    currTrade.Id = reader.GetString(0);
                    currTrade.Type = reader.GetString(1);
                    currTrade.MinimumDamage = reader.GetFloat(2);
                    currTrade.CardToTrade = reader.GetString(3);
                }
                if (currTrade.Id == null) return null;
            }
            return currTrade;
        }

        public bool CheckRequirements(Trade currTrade, string cardId)
        {
            IDatabase getCardStats = this.dbQuery.NewCommand(@"SELECT cardtype, damage, indeck FROM cards WHERE id = @cardid");
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
            if (type == CardType.SPELL && currTrade.Type == "monster" ||
                type != CardType.SPELL && currTrade.Type == "spell") return false;
            if (damage < currTrade.MinimumDamage || indeck) return false;

            return true;

        }
    }
}
