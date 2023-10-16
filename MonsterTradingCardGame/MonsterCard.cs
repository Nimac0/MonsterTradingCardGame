using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    enum MonsterType
    {
        GOBLIN = 0,
        WIZZARD,
        ORKS,
        DRAGON,
        KNIGHT,
        KRAKEN,
        ELF
    }

    internal class MonsterCard : Card
    {

        public IDictionary<MonsterType, string> MonsterNames = new Dictionary<MonsterType, string>()
        {
            { MonsterType.GOBLIN, "Goblin" },
            { MonsterType.DRAGON, "Dragon" },
            { MonsterType.WIZZARD, "Wizzard" },
            { MonsterType.ORKS, "Orks" },
            { MonsterType.KNIGHT, "Knight" },
            { MonsterType.KRAKEN, "Kraken" },
            { MonsterType.ELF, "Elf" }
        };

        public MonsterType Type;
        

        public MonsterCard(Element cardElement, MonsterType monsterType) : base(cardElement)
        {
            this.Type = monsterType;
            this.CardName = ElementType[cardElement] + MonsterNames[monsterType];
            Random rand = new Random((int)DateTime.UtcNow.Ticks);
            this.Damage = (rand.Next(15)) * 5;
        }
    }
}

