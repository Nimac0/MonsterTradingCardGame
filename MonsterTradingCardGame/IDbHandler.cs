using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal interface IDbHandler
    {
        public void RegisterUser(string requestBody)
        {
            //TODO db request -> make entry
        }

        public string GetUserData(string username)
        {
            return "placeholder"; //TODO replace with db request
        }

        public void UpdateUserData(string username)
        {
            //TODO db request to update userdata 
        }
    }
}
