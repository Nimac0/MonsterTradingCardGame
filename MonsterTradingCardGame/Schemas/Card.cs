using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonsterTradingCardGame.Schemas
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

    public class Card
    {
        public Card(string cardId, Element cardElement, CardType monsterType, float damage, bool inDeck, bool inTrade)
        {
            Id = cardId;
            CardElement = cardElement;
            Type = monsterType;
            CardName = cardElement.ToString() + monsterType.ToString();
            Damage = damage;
            inPlayingDeck = inDeck;
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
