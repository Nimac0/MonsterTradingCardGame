using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum Element
{
    WATER = 0,
    FIRE,
    NORMAL
}

namespace MonsterTradingCardGame
{
    public abstract class Card
    {

        private Element _cardElement;
        private string _cardName;
        private int _cost;


    }
}
