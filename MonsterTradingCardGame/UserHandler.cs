using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class UserHandler
    {
        public string RegisterUser(string requestBody)
        {
            return "registered user";
        }

        public string GetUserData(string username)
        {
            return "placeholder user data"; //TODO replace with db request
        }

        public string UpdateUserData(string username)
        {
            return "updated user";
            //TODO db request to update userdata 
        }
    }
}
