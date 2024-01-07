using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Schemas
{
    public class Trade
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public float? MinimumDamage { get; set; }
        public string? CardToTrade { get; set; }
    }
}
