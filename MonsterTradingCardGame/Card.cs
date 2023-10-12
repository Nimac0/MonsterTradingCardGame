using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonsterTradingCardGame
{
    public enum Element
    {
        WATER = 0,
        FIRE,
        NORMAL
    }

    public abstract class Card
    {
        public Card(Element cardElement, string cardName, int cost)
        {
            this.CardElement = cardElement;
            this.CardName = cardName;
            this.Cost = cost;
        }

        public Element CardElement { get; set; }
        public string CardName { get; set; }
        public int Cost { get; set; }


    }
}
