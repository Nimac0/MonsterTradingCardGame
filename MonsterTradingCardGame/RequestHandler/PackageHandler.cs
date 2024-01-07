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
    public class PackageHandler
    {
        public DbQuery dbQuery = new DbQuery();

        public string CreateCardPackage(string requestBody, string authToken)
        {
            string authorizedUser = SessionHandler.Instance.GetUsernameByToken(authToken);

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
                DbQuery checkIfCardExists = this.dbQuery.NewCommand(@"SELECT * FROM cards WHERE id = @id");
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

            DbQuery createId = this.dbQuery.NewCommand(@"INSERT INTO packages (id) VALUES (@id) RETURNING id;");
            createId.AddParameterWithValue("id", DbType.String, packageId);

            string id = (string)(createId.ExecuteScalar() ?? "");
            if (id == "") return "???";

            CardHandler cardHandler = new CardHandler();
            foreach (Card card in package)
            {
                SetCardAttributes(card);

                if (!cardHandler.CreateCard(card, packageId)) return "???";
            }

            return Response.CreateResponse("201", "Created", "", "application/json");
        }

        public string BuyCardPackage(string authToken)
        {
            string authorizedUser = SessionHandler.Instance.GetUsernameByToken(authToken);

            if (string.Equals(authorizedUser, "")) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            DbQuery checkCoins = this.dbQuery.NewCommand(@"SELECT coins FROM users WHERE username = @username;");
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
            DbQuery checkAvailability = this.dbQuery.NewCommand(@"SELECT id FROM packages ORDER BY RANDOM() LIMIT 1;");
            using (IDataReader reader = checkAvailability.ExecuteReader())
            {
                if (reader.Read())
                {
                    packageId = reader.GetString(0);
                }
            }
            if (packageId == "") return Response.CreateResponse("404", "Not Found", "", "application/json");

            int? userId = SessionHandler.Instance.GetIdByUsername(authorizedUser);
            DbQuery spendCoins = this.dbQuery.NewCommand(@"UPDATE users SET coins = @coins WHERE id = @userid;");
            spendCoins.AddParameterWithValue("coins", DbType.Int32, coins);
            spendCoins.AddParameterWithValue("userid", DbType.Int32, userId);

            spendCoins.ExecuteNonQuery(); // TODO add mutex

            List<Card> boughtCards = new List<Card>();
            DbQuery getCards = this.dbQuery.NewCommand(@"SELECT * FROM cards WHERE packageid = @packageid;");
            getCards.AddParameterWithValue("packageid", DbType.String, packageId);

            using (IDataReader reader = getCards.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card card = new Card(reader.GetString(0), (Element)reader.GetInt32(1), (CardType)reader.GetInt32(2), reader.GetFloat(3), reader.GetBoolean(4), reader.GetBoolean(5), reader.GetString(7));
                    boughtCards.Add(card);
                }
            }

            foreach (Card card in boughtCards)
            {
                if (!ChangeCardOwner(userId, card.Id)) return "???";
            }

            DbQuery deletePackage = this.dbQuery.NewCommand(@"DELETE FROM packages WHERE id = @id");
            deletePackage.AddParameterWithValue("id", DbType.String, packageId);
            deletePackage.ExecuteNonQuery();

            return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(boughtCards, Formatting.Indented), "application/json");
        }

        public bool ChangeCardOwner(int? UserId, string cardId) //changes owner of card with given cardid also removes packageid
        {
            DbQuery updateCardsUserId = this.dbQuery.NewCommand(@"UPDATE cards SET userid = @userid, packageid = @packageid WHERE id = @cardid;");
            updateCardsUserId.AddParameterWithValue("userid", DbType.Int32, UserId);
            updateCardsUserId.AddParameterWithValue("packageid", DbType.String, "");
            updateCardsUserId.AddParameterWithValue("cardid", DbType.String, cardId);

            if (updateCardsUserId.ExecuteNonQuery() == 0) return false;

            return true;
        }

        public void SetCardAttributes(Card card) //splits name and assigns element and type
        {
            if (card.Name.Count(char.IsUpper) > 1)
            {
                string[] elementAndType = card.SplitCardName(card.Name);
                card.SetElement(elementAndType[0]);
                card.SetType(elementAndType[1]);
                return;
            }
            card.SetElement("Regular");
            card.SetType(card.Name);
        }
    }
}
