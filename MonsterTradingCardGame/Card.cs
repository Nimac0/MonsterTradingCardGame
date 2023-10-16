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
        public IDictionary<Element, string> ElementType = new Dictionary<Element, string>()
        {
            { Element.WATER, "Water" },
            { Element.FIRE, "Fire" },
            { Element.NORMAL, "Regular" }
        };

        public Card(Element cardElement)
        {
            this.CardElement = cardElement;
        }

        public Element CardElement { get; set; }
        public string CardName { get; set; }
        public int Damage { get; set; }
    }
}
