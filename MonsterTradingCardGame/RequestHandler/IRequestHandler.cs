using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Requests
{
    internal interface IRequestHandler
    {
        string Get();
        string Post();
        string Put();
        string Delete();
    }
}
