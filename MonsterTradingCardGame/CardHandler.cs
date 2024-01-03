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

            DbHandler createId = new DbHandler(@"INSERT INTO packages (id) VALUES (@id) RETURNING id;");
            createId.AddParameterWithValue("id", DbType.String, packageId);

            string id = (string)(createId.ExecuteScalar() ?? "");
            if (id == "") return "???";

            foreach(Card card in package)
            {
                if(!this.CreateCard(card, packageId)) return Response.CreateResponse("409", "Conflict", "", "application/json");
            }

            return Response.CreateResponse("201", "Created", "", "application/json");
        }

        public string BuyCardPackage(string requestBody)
        {
            return "bought package";
        }

        public bool CreateCard(Card newCard, string packageId)// TODO fix 409 message (maybe add list instead of each card)
        {
            DbHandler checkIfCardExists = new DbHandler(@"SELECT * FROM cards WHERE id = @id");
            checkIfCardExists.AddParameterWithValue("id", DbType.String, newCard.Id);
            using (IDataReader reader = checkIfCardExists.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (reader.GetString(0) != null) return false;
                }
            }
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
