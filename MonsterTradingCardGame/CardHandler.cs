using System;
using System.Collections.Generic;
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

        public string GetCards(string requestBody) //TODO make return Card[]
        {
            return "card data";
        }

        public string GetDeck(string requestBody) //TODO make return Card[]
        {
            return "deck data";
        }

        public string CreateDeck(string requestBody) //TODO make return Card[]
        {
            return "deck created";
        }
    }
}
