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

        public List<Card> GetCards(string authToken) 
        {
            DbHandler getIdByUsername = new DbHandler(@"SELECT users.id FROM users WHERE username = @username");
            string username = SessionHandler.getUsernameByToken(authToken);
            Console.WriteLine("current username: "+ username);
            getIdByUsername.AddParameterWithValue("username", DbType.String, username);

            int? userId = null;
            using (IDataReader reader = getIdByUsername.ExecuteReader())
            {
                if (reader.Read())
                {
                    User newUser = new User()
                    {
                        Id = reader.GetInt32(0)
                    };
                    userId = newUser.Id;
                    Console.WriteLine(newUser.Id);
                }
            }
            List<Card> cardCollection = new List<Card>();
            DbHandler dbHandler = new DbHandler(@"SELECT cards.id,element,cardtype,damage,indeck,intrade,userid FROM cards WHERE userid = @userid;");
            dbHandler.AddParameterWithValue("userId", DbType.Int32, userId);

            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card newCard = new Card((Element)reader.GetInt32(1), (CardType)reader.GetInt32(2), reader.GetFloat(3));
                    cardCollection.Add(newCard);
                }
                return cardCollection;
            }
        }

        public string GetDeck(string requestBody) 
        {
            return "deck data";
        }

        public string CreateDeck(string requestBody)
        {
            return "deck created";
        }
    }
}
