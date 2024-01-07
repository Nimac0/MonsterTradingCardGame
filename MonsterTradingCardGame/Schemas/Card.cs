using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        ORK,
        DRAGON,
        KNIGHT,
        KRAKEN,
        ELF,
        SPELL,
        TROLL
    }


    public class Card
    {

        public Dictionary<string, Element> elementMap = new Dictionary<string, Element>()
        {
            { "Water", Element.WATER },
            { "Fire", Element.FIRE },
            { "Regular", Element.NORMAL }
        };
        public Dictionary<string, CardType> typeMap = new Dictionary<string, CardType>()
        {
            { "Goblin", CardType.GOBLIN },
            { "Troll", CardType.TROLL },
            { "Elf", CardType.ELF },
            { "Spell", CardType.SPELL },
            { "Kraken", CardType.KRAKEN },
            { "Wizzard", CardType.WIZZARD },
            { "Dragon", CardType.DRAGON },
            { "Knight", CardType.KNIGHT },
            { "Ork", CardType.ORK }
        };

        public Card(string cardId, Element cardElement, CardType monsterType, float damage, bool inDeck, bool inTrade, string cardName)
        {
            Id = cardId;
            CardElement = cardElement;
            Type = monsterType;
            Name = cardName;
            Damage = damage;
            inPlayingDeck = inDeck;
            this.inTrade = inTrade;
        }

        public string Id { get; set; }
        public CardType Type { get; set; }
        public Element CardElement { get; set; } = Element.NORMAL;
        public string Name { get; set; }
        public float Damage { get; set; }
        public bool inPlayingDeck { get; set; }
        public bool inTrade { get; set; }


        public string[] SplitCardName(string cardName)
        {
        //https://stackoverflow.com/questions/773303/splitting-camelcase

            string[] words = Regex.Matches(cardName, "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)")
                .OfType<Match>()
                .Select(m => m.Value)
                .ToArray();
            return words;
        }

        public void SetElement(string element)
        {
            this.CardElement = elementMap[element];
        }
        public void SetType(string type)
        {
            this.Type = typeMap[type];
        }
    }
}
