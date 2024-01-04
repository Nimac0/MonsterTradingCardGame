using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class CardHandler
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
                if(currCardIds.Contains(card.Id)) return Response.CreateResponse("409", "Conflict", "", "application/json"); ;
                currCardIds.Add(card.Id);
                DbHandler checkIfCardExists = new DbHandler(@"SELECT * FROM cards WHERE id = @id");
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

            DbHandler createId = new DbHandler(@"INSERT INTO packages (id) VALUES (@id) RETURNING id;");
            createId.AddParameterWithValue("id", DbType.String, packageId);

            string id = (string)(createId.ExecuteScalar() ?? "");
            if (id == "") return "???";

            foreach (Card card in package)
            {
                if(this.CreateCard(card, packageId));
            }

            return Response.CreateResponse("201", "Created", "", "application/json");
        }

        public string BuyCardPackage(string authToken)
        {
            string authorizedUser = SessionHandler.GetUsernameByToken(authToken);

            if (string.Equals(authorizedUser, "")) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            DbHandler checkCoins = new DbHandler(@"SELECT coins FROM users WHERE username = @username;");
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
            DbHandler checkAvailability = new DbHandler(@"SELECT id FROM packages ORDER BY RANDOM() LIMIT 1;");
            using (IDataReader reader = checkAvailability.ExecuteReader())
            {
                if (reader.Read())
                {
                    packageId = reader.GetString(0);
                }
            }
            if(packageId == "") return Response.CreateResponse("404", "Not Found", "", "application/json");

            int? userId = SessionHandler.GetIdByUsername(authorizedUser);
            DbHandler spendCoins = new DbHandler(@"UPDATE users SET coins = @coins WHERE id = @userid;");
            spendCoins.AddParameterWithValue("coins", DbType.Int32, coins);
            spendCoins.AddParameterWithValue("userid", DbType.Int32, userId);

            spendCoins.ExecuteNonQuery();

            List<Card> boughtCards = new List<Card>();
            DbHandler getCards = new DbHandler(@"SELECT * FROM cards WHERE packageid = @packageid;");
            getCards.AddParameterWithValue("packageid", DbType.String, packageId);

            using (IDataReader reader = getCards.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card card = new Card(reader.GetString(0), (Element)reader.GetInt32(1), (CardType)reader.GetInt32(2), reader.GetFloat(3), reader.GetBoolean(4), reader.GetBoolean(5));
                    boughtCards.Add(card);
                }
            }

            foreach(Card card in boughtCards)
            {
                ChangeCardOwner(userId, card.Id);
            }

            DbHandler deletePackage = new DbHandler(@"DELETE FROM packages WHERE id = @id");
            deletePackage.AddParameterWithValue("id", DbType.String, packageId);
            deletePackage.ExecuteNonQuery();

            return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(boughtCards, Formatting.Indented), "application/json");
        }

        public void ChangeCardOwner(int? UserId, string cardId)
        {
            DbHandler updateCardsUserId = new DbHandler(@"UPDATE cards SET userid = @userid, packageid = @packageid WHERE id = @cardid;");
            updateCardsUserId.AddParameterWithValue("userid", DbType.Int32, UserId);
            updateCardsUserId.AddParameterWithValue("packageid", DbType.String, "");
            updateCardsUserId.AddParameterWithValue("cardid", DbType.String, cardId);

            updateCardsUserId.ExecuteNonQuery();
        }

        public bool CreateCard(Card newCard, string packageId)
        {
            DbHandler createCard = new DbHandler(@"INSERT INTO cards (id, element, cardtype, damage, indeck, intrade, userid, cardname, packageid)"
            + "VALUES (@id, @element, @cardtype, @damage, @indeck, @intrade, @userid, @cardname, @packageid)");

            createCard.AddParameterWithValue("id", DbType.String, newCard.Id);
            createCard.AddParameterWithValue("element", DbType.Int32, (int)newCard.CardElement);
            createCard.AddParameterWithValue("cardtype", DbType.Int32, (int)newCard.Type);
            createCard.AddParameterWithValue("damage", DbType.Decimal, newCard.Damage);
            createCard.AddParameterWithValue("indeck", DbType.Boolean, newCard.inPlayingDeck);
            createCard.AddParameterWithValue("intrade", DbType.Boolean, newCard.inTrade);
            createCard.AddParameterWithValue("userid", DbType.Int32, null);
            createCard.AddParameterWithValue("cardname", DbType.String, newCard.CardName);
            createCard.AddParameterWithValue("packageid", DbType.String, packageId);

            string id = (string)(createCard.ExecuteScalar() ?? "");
            if (id != "") return true;

            return false;
        }

        public List<Card> GetCardsOrDeck(string authToken, bool deckRequested) 
        {
            int? userId = SessionHandler.GetIdByUsername(SessionHandler.GetUsernameByToken(authToken));
            if (userId == null) return null;

            List<Card> cardCollection = new List<Card>();
            DbHandler dbHandler = new DbHandler(deckRequested ? @"SELECT cards.id,element,cardtype,damage,indeck,intrade,userid, cardname FROM cards WHERE userid = @userid AND indeck = TRUE;"
               : @"SELECT cards.id,element,cardtype,damage,indeck,intrade,userid, cardname FROM cards WHERE userid = @userid AND indeck = FALSE;");
            dbHandler.AddParameterWithValue("userId", DbType.Int32, userId);

            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card newCard = new Card(reader.GetString(0), (Element)reader.GetInt32(1), (CardType)reader.GetInt32(2), reader.GetFloat(3), reader.GetBoolean(4), reader.GetBoolean(5));

                    cardCollection.Add(newCard);
                }
                return cardCollection;
            }
        }

        public string ChooseDeck(string requestBody, string authToken)
        {
            int? userId = SessionHandler.GetIdByUsername(SessionHandler.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            DbHandler dbHandler = new DbHandler(@"SELECT * FROM cards WHERE id = @cardid AND userid = @userid;");
            List<string>? cardIds = JsonConvert.DeserializeObject<List<string>>(requestBody);
            if(cardIds.Count() != 4) return Response.CreateResponse("400", "Bad Request", "", "application/json");

            int counter = 0;
            foreach(string cardId in cardIds)
            {
                dbHandler.AddParameterWithValue("cardid", DbType.String, cardId);
                dbHandler.AddParameterWithValue("userid", DbType.Int32, userId);

                using (IDataReader reader = dbHandler.ExecuteReader()){
                    if (reader.Read()){
                        counter++;
                        Console.WriteLine(counter);
                    }
                }
            }
            if (counter != 4) return Response.CreateResponse("403", "Forbidden", "", "application/json");
            DbHandler clearDeck = new DbHandler(@"UPDATE cards SET indeck = FALSE WHERE userid = @userid;");
            clearDeck.AddParameterWithValue("userid", DbType.Int32, userId);
            clearDeck.ExecuteNonQuery();
            DbHandler putInDeck = new DbHandler(@"UPDATE cards SET indeck = TRUE WHERE id IN (@id1, @id2, @id3, @id4);");
            
            int i = 1;
            foreach (string cardId in cardIds)
            {
                Console.WriteLine(cardId);
                putInDeck.AddParameterWithValue("id" + i++, DbType.String, cardId);
            }
            putInDeck.ExecuteNonQuery();
            return Response.CreateResponse("200", "OK", "", "application/json");
        }
    }
}
