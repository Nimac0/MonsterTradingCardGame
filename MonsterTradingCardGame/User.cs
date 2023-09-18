using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class User
    {
        public string UserName { get; set; }
        public User(string UserName)
        {
            this.UserName = UserName;
        }
    }
}
