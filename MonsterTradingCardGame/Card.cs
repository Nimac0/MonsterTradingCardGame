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

    internal class Card
    {
        public Card(string cardId, Element cardElement, CardType monsterType, float damage, bool inDeck, bool inTrade)
        {
            this.Id = cardId;
            this.CardElement = cardElement;
            this.Type = monsterType;
            this.CardName = cardElement.ToString() + monsterType.ToString();
            this.Damage = damage;
            this.inPlayingDeck = inDeck;
            this.inTrade = inTrade;
        }
 
        public string Id { get; set; }
        public CardType Type { get; set; }
        public Element CardElement { get; set; }
        public string CardName { get; set; }
        public float Damage { get; set; }
        public bool inPlayingDeck { get; set; }
        public bool inTrade { get; set; }
    }
}
