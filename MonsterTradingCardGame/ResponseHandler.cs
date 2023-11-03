using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class ResponseHandler
    {
        public void ErrorHandler(int errorCode)
        {
            switch(errorCode)
            {
                case 422:
                    break;
            }
        }
    }
}
