using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class TradeHandler
    {
        public string GetTrades()
        {
            return "get trades";
        }

        public string PostTrade(string requestBody)
        {
            return "trades posted";
        }

        public string DeleteTrade(string tradeId)
        {
            return "trade deleted";
        }

        public string StartTrade(string tradeId)
        {
            return "trade started";
        }
    }
}
