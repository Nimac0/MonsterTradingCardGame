using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal interface ICardHandler
    {
        public void CreateCardPackage(string requestBody)
        {

        }

        public void BuyCardPackage(string requestBody)
        {

        }

        public void GetCards(string requestBody) //TODO make return Card[]
        {

        }

        public void GetDeck(string requestBody) //TODO make return Card[]
        {

        }

        public void CreateDeck(string requestBody) //TODO make return Card[]
        {

        }
    }
}
