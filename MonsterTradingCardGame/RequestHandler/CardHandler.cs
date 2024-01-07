using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.http;
using MonsterTradingCardGame.Schemas;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.RequestHandler
{
    public class CardHandler
    {
        public IDatabase dbQuery = new DbQuery();
        public bool CreateCard(Card newCard, string packageId)
        {
            IDatabase createCard = this.dbQuery.NewCommand(@"INSERT INTO cards (id, element, cardtype, damage, indeck, intrade, userid, cardname, packageid)"
            + "VALUES (@id, @element, @cardtype, @damage, @indeck, @intrade, @userid, @cardname, @packageid) RETURNING id");

            createCard.AddParameterWithValue("id", DbType.String, newCard.Id);
            createCard.AddParameterWithValue("element", DbType.Int32, (int)newCard.CardElement);
            createCard.AddParameterWithValue("cardtype", DbType.Int32, (int)newCard.Type);
            createCard.AddParameterWithValue("damage", DbType.Decimal, newCard.Damage);
            createCard.AddParameterWithValue("indeck", DbType.Boolean, newCard.inPlayingDeck);
            createCard.AddParameterWithValue("intrade", DbType.Boolean, newCard.inTrade);
            createCard.AddParameterWithValue("userid", DbType.Int32, null);
            createCard.AddParameterWithValue("cardname", DbType.String, newCard.Name);
            createCard.AddParameterWithValue("packageid", DbType.String, packageId);

            string id = (string)(createCard.ExecuteScalar() ?? "");
            if (id != "") return true;

            return false;
        }

        public List<ResponseCard> GetCardsOrDeck(string authToken, bool deckRequested)
        {
            int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
            if (userId == null) return null;

            List<ResponseCard> cardCollection = new List<ResponseCard>();
            IDatabase dbHandler = this.dbQuery.NewCommand(deckRequested ? @"SELECT cards.id,damage,cardname FROM cards WHERE userid = @userid AND indeck = TRUE;"
               : @"SELECT cards.id,damage,cardname FROM cards WHERE userid = @userid AND indeck = FALSE;");
            dbHandler.AddParameterWithValue("userId", DbType.Int32, userId);

            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                while (reader.Read())
                {
                    ResponseCard newCard = new ResponseCard()
                    {
                        Id = reader.GetString(0),
                        Damage = reader.GetFloat(1),
                        Name = reader.GetString(2)
                    };
                    cardCollection.Add(newCard);
                }
                return cardCollection;
            }
        }

        public string ChooseDeck(string requestBody, string authToken)
        {
            int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
            if (userId == null) return Response.CreateResponse("401", "Unauthorised", "", "application/json");

            List<string>? cardIds = JsonConvert.DeserializeObject<List<string>>(requestBody);
            if (cardIds.Count() != 4) return Response.CreateResponse("400", "Bad Request", "", "application/json");

            int counter = 0;
            foreach (string cardId in cardIds)
            {
                IDatabase dbHandler = this.dbQuery.NewCommand(@"SELECT * FROM cards WHERE id = @cardid AND userid = @userid;");

                dbHandler.AddParameterWithValue("cardid", DbType.String, cardId);
                dbHandler.AddParameterWithValue("userid", DbType.Int32, userId);

                using (IDataReader reader = dbHandler.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0));
                        counter++;
                        Console.WriteLine(counter);
                    }
                }
            }
            Console.WriteLine(counter);
            if (counter != 4) return Response.CreateResponse("403", "Forbidden", "", "application/json");
            IDatabase clearDeck = this.dbQuery.NewCommand(@"UPDATE cards SET indeck = FALSE WHERE userid = @userid;");
            clearDeck.AddParameterWithValue("userid", DbType.Int32, userId);
            clearDeck.ExecuteNonQuery();
            IDatabase putInDeck = this.dbQuery.NewCommand(@"UPDATE cards SET indeck = TRUE WHERE id IN (@id1, @id2, @id3, @id4);");

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
