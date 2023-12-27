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

        public string GetCards(string requestBody) 
        {
            using IDbConnection connection = new NpgsqlConnection(DbHandler.connectionstring);
            using IDbCommand command = connection.CreateCommand();
            connection.Open();

            command.CommandText = "SELECT cards.id,element,cardtype,damage,indeck,intrade,userid FROM cards WHERE userid = @userid;"; //TODO make function to get id based on username
            return "card data";
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
