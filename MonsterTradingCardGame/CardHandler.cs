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
        public string CreateCardPackage(string requestBody)
        {
            return "package created";
        }

        public string BuyCardPackage(string requestBody)
        {
            return "bought package";
        }

        public List<Card> GetCardsOrDeck(string authToken, bool deckRequested) 
        {
            int? userId = SessionHandler.GetIdByUsername(SessionHandler.GetUsernameByToken(authToken));

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

            foreach (string cardId in cardIds)
            {
                DbHandler putInDeck = new DbHandler(@"UPDATE cards SET indeck = TRUE WHERE id = @cardid;");
                Console.WriteLine(cardId);
                putInDeck.AddParameterWithValue("cardid", DbType.String, cardId);
                putInDeck.ExecuteNonQuery();
            }
            return Response.CreateResponse("200", "OK", "", "application/json");
        }
    }
}
