using Newtonsoft.Json;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class UserHandler
    {
        public string CreateOrUpdateUser(string username, string requestBody, string authToken, bool isNew)
        {
            User newUser = new User();
            try
            {
                newUser = JsonConvert.DeserializeObject<User>(requestBody);
            }
            catch(Exception e)
            {
               Console.WriteLine(e.Message);
            }

            if (!isNew)
            { 
                if (GetUserData(username, authToken, true) == null) return "user doesnt exists";
            }
            if ((GetUserData(newUser.Username, authToken, false) != null) && (username != newUser.Username)) return "user already exists cannot change name";

            DbHandler dbHandler = new DbHandler(isNew ? "INSERT INTO users (username, password, coins, elo) " +
                "VALUES (@username, @password, @coins, @elo) RETURNING id"
                : "UPDATE users SET username = @username, password = @password, coins = @coins, elo = @elo WHERE username = @oldusername");
                    
            dbHandler.AddParameterWithValue("username", DbType.String, newUser.Username);
            dbHandler.AddParameterWithValue("password", DbType.String, newUser.Password);
            dbHandler.AddParameterWithValue("coins", DbType.Int32, 20);
            dbHandler.AddParameterWithValue("elo", DbType.Int32, 100);
            if(!isNew) dbHandler.AddParameterWithValue("oldusername", DbType.String, username);

            if (isNew)
            {
                int id = (int)(dbHandler.ExecuteScalar() ?? 0);
                if (id != 0) return "registered user";
            }
            if(!isNew)
            {
                dbHandler.ExecuteNonQuery();
                return "updated user";
            }

            return "???";
        }

        public string GetUserData(string username, string authToken, bool authNeeded) // TODO: add join for cards, check if user is authorized
        {
            authToken = authToken.TrimEnd('\r', '\n');
            string authorizedUser = "";

            if (!SessionHandler.tokenMap.ContainsKey(authToken))
            {
                return "token not found";
            }

            if (!SessionHandler.userMap.ContainsKey(SessionHandler.tokenMap[authToken]))
            {
                return "id not found";
            }
            authorizedUser = SessionHandler.userMap[SessionHandler.tokenMap[authToken]].Username;
            if (!string.Equals(username, authorizedUser) && authNeeded) return "user not authorized";
            DbHandler dbHandler = new DbHandler(@"SELECT * FROM users WHERE username = @username");

            dbHandler.AddParameterWithValue("username", DbType.String, username);
            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                if (reader.Read())
                {
                    User newUser = new User()
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        Coins = reader.GetInt32(3),
                        EloValue = reader.GetInt32(4)
                    };
                    return JsonConvert.SerializeObject(newUser);
                }
            }

            return null;
        }
    }
}
