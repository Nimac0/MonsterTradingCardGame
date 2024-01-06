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
    internal class PackageHandler
    {
        public string CreateCardPackage(string requestBody, string authToken)
        {
            string authorizedUser = SessionHandler.GetUsernameByToken(authToken);

            if (string.Equals(authorizedUser, "")) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            if (!string.Equals(authorizedUser, "admin")) return Response.CreateResponse("403", "Forbidden", "", "application/json");

            List<Card> package = JsonConvert.DeserializeObject<List<Card>>(requestBody);
            string packageId = Guid.NewGuid().ToString();

            int errcounter = 0;
            List<string> currCardIds = new List<string>();
            foreach (Card card in package)
            {
                if (currCardIds.Contains(card.Id)) return Response.CreateResponse("409", "Conflict", "", "application/json"); ;
                currCardIds.Add(card.Id);
                DbQuery checkIfCardExists = new DbQuery(@"SELECT * FROM cards WHERE id = @id");
                checkIfCardExists.AddParameterWithValue("id", DbType.String, card.Id);
                using (IDataReader reader = checkIfCardExists.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.GetString(0) != null) errcounter++;
                    }
                }
            }
            if (errcounter != 0) return Response.CreateResponse("409", "Conflict", "", "application/json");

            DbQuery createId = new DbQuery(@"INSERT INTO packages (id) VALUES (@id) RETURNING id;");
            createId.AddParameterWithValue("id", DbType.String, packageId);

            string id = (string)(createId.ExecuteScalar() ?? "");
            if (id == "") return "???";

            CardHandler cardHandler = new CardHandler();
            foreach (Card card in package)
            {
                if (!cardHandler.CreateCard(card, packageId)) return "???";
            }

            return Response.CreateResponse("201", "Created", "", "application/json");
        }

        public string BuyCardPackage(string authToken)
        {
            string authorizedUser = SessionHandler.GetUsernameByToken(authToken);

            if (string.Equals(authorizedUser, "")) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            DbQuery checkCoins = new DbQuery(@"SELECT coins FROM users WHERE username = @username;");
            checkCoins.AddParameterWithValue("username", DbType.String, authorizedUser);
            int coins = 0;
            using (IDataReader reader = checkCoins.ExecuteReader())
            {
                if (reader.Read())
                {
                    coins = reader.GetInt32(0);
                    if (coins < 5) return Response.CreateResponse("403", "Forbidden", "", "application/json");
                    coins -= 5;
                }
            }

            string packageId = "";
            DbQuery checkAvailability = new DbQuery(@"SELECT id FROM packages ORDER BY RANDOM() LIMIT 1;");
            using (IDataReader reader = checkAvailability.ExecuteReader())
            {
                if (reader.Read())
                {
                    packageId = reader.GetString(0);
                }
            }
            if (packageId == "") return Response.CreateResponse("404", "Not Found", "", "application/json");

            int? userId = SessionHandler.GetIdByUsername(authorizedUser);
            DbQuery spendCoins = new DbQuery(@"UPDATE users SET coins = @coins WHERE id = @userid;");
            spendCoins.AddParameterWithValue("coins", DbType.Int32, coins);
            spendCoins.AddParameterWithValue("userid", DbType.Int32, userId);

            spendCoins.ExecuteNonQuery(); // TODO add mutex

            List<Card> boughtCards = new List<Card>();
            DbQuery getCards = new DbQuery(@"SELECT * FROM cards WHERE packageid = @packageid;");
            getCards.AddParameterWithValue("packageid", DbType.String, packageId);

            using (IDataReader reader = getCards.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card card = new Card(reader.GetString(0), (Element)reader.GetInt32(1), (CardType)reader.GetInt32(2), reader.GetFloat(3), reader.GetBoolean(4), reader.GetBoolean(5));
                    boughtCards.Add(card);
                }
            }

            foreach (Card card in boughtCards)
            {
                if (!ChangeCardOwner(userId, card.Id)) return "???";
            }

            DbQuery deletePackage = new DbQuery(@"DELETE FROM packages WHERE id = @id");
            deletePackage.AddParameterWithValue("id", DbType.String, packageId);
            deletePackage.ExecuteNonQuery();

            return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(boughtCards, Formatting.Indented), "application/json");
        }

        public bool ChangeCardOwner(int? UserId, string cardId) //changes owner of card with given cardid also removes packageid
        {
            DbQuery updateCardsUserId = new DbQuery(@"UPDATE cards SET userid = @userid, packageid = @packageid WHERE id = @cardid;");
            updateCardsUserId.AddParameterWithValue("userid", DbType.Int32, UserId);
            updateCardsUserId.AddParameterWithValue("packageid", DbType.String, "");
            updateCardsUserId.AddParameterWithValue("cardid", DbType.String, cardId);

            if (updateCardsUserId.ExecuteNonQuery() == 0) return false;

            return true;
        }
    }
}
