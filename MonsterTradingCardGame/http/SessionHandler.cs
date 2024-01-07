using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.RequestHandler;
using MonsterTradingCardGame.Schemas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.http
{
    public class SessionHandler
    {
        public DbQuery dbQuery = new DbQuery();
        public static Dictionary<string, int> tokenMap = new Dictionary<string, int>(); // token > id > user object
        public static Dictionary<int, User> userMap = new Dictionary<int, User>();
        private static SessionHandler instance = null;
        private static readonly object padlock = new object();
        SessionHandler()
        {
        }

        public static SessionHandler Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SessionHandler();
                    }
                    return instance;
                }
            }
        }

        public string LoginUser(string requestBody)
        {
            User newUserData = new User();
            try
            {
                newUserData = JsonConvert.DeserializeObject<User>(requestBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            DbQuery dbHandler = this.dbQuery.NewCommand(@"SELECT * FROM users WHERE username = @username AND password = @password");
            dbHandler.AddParameterWithValue("username", DbType.String, newUserData.Username);
            dbHandler.AddParameterWithValue("password", DbType.String, newUserData.Password);
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
                    if (tokenMap.ContainsValue((int)newUser.Id)) return Response.CreateResponse("200", "OK", "", "application/json");
                    string token = CreateToken(newUser.Username);
                    tokenMap.Add(token, (int)newUser.Id);
                    userMap.Add((int)newUser.Id, newUser);

                    return Response.CreateResponse("200", "OK", "", "application/json");
                }
            }
            return Response.CreateResponse("401", "Unauthorised", "", "application/json");
        }

        public string CreateToken(string username) //this is not the ideal way of doing tokens bc predictable and insecure but the curl script uses tokens like this and i dont want to change the script that much :P
        {
            string token = "Bearer " + username + "-mtcgToken";
            return token;
        }

        public string GetUsernameByToken(string authToken)
        {
            authToken = authToken.TrimEnd('\r', '\n');
            Console.WriteLine(authToken);

            if (!tokenMap.ContainsKey(authToken))
            {
                return "";
            }

            if (!userMap.ContainsKey(tokenMap[authToken]))
            {
                return "";
            }
            return userMap[tokenMap[authToken]].Username;
        }

        public int? GetIdByUsername(string username)
        {
            DbQuery getIdByUsername = this.dbQuery.NewCommand(@"SELECT users.id FROM users WHERE username = @username");

            getIdByUsername.AddParameterWithValue("username", DbType.String, username);

            using (IDataReader reader = getIdByUsername.ExecuteReader())
            {
                if (reader.Read())
                {
                    User newUser = new User()
                    {
                        Id = reader.GetInt32(0)
                    };
                    return newUser.Id;
                }
            }
            return null;
        }
    }
}
