using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class Trade
    {
        public string? Id { get; set; }
        public string? RequiredType { get; set; }
        public float? RequiredDamage { get; set; }
        public string? CardId { get; set; }
    }
}
