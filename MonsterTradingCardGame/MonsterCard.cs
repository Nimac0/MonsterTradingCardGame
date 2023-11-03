using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{

    internal class MonsterCard : Card
    {

        public IDictionary<CardType, string> MonsterNames = new Dictionary<CardType, string>()
        {
            { CardType.GOBLIN, "Goblin" },
            { CardType.DRAGON, "Dragon" },
            { CardType.WIZZARD, "Wizzard" },
            { CardType.ORKS, "Orks" },
            { CardType.KNIGHT, "Knight" },
            { CardType.KRAKEN, "Kraken" },
            { CardType.ELF, "Elf" }
        };

        public MonsterCard(Element cardElement, CardType monsterType) : base(cardElement)
        {
            this.Type = monsterType;
            this.CardName = ElementType[cardElement] + MonsterNames[monsterType];
            Random rand = new Random((int)DateTime.UtcNow.Ticks);
            this.Damage = (rand.Next(15)) * 5;
        }
    }
}

