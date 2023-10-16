using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class SpellCard : Card
    {
        public SpellCard(Element cardElement) : base(cardElement)
        {
            base.CardName = ElementType[cardElement] + "Spell";
            Random rand = new Random((int)DateTime.UtcNow.Ticks);
            this.Damage = (rand.Next(15)) * 5;
        }
    }
}
