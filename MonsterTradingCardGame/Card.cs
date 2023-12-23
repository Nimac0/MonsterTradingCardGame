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

    public enum CardType
    {
        GOBLIN = 0,
        WIZZARD,
        ORKS,
        DRAGON,
        KNIGHT,
        KRAKEN,
        ELF,
        SPELL
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
            this.inTrade = false;
            this.inPlayingDeck = false;
        }

        public bool inPlayingDeck { get; set; }
        public bool inTrade { get; set; }
        public CardType Type { get; set; }
        public Element CardElement { get; set; }
        public string CardName { get; set; }
        public int Damage { get; set; }
    }
}
