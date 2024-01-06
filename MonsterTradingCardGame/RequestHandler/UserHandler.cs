using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.http;
using MonsterTradingCardGame.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MonsterTradingCardGame.RequestHandler
{
    internal class UserHandler
    {
        public string CreateUser(string username, string requestBody, string authToken)
        {
            User newUser = new User();
            try
            {
                newUser = JsonConvert.DeserializeObject<User>(requestBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (GetUserData(newUser.Username, authToken, false) != null) return Response.CreateResponse("409", "Conflict", "", "application/json");

            DbQuery dbHandler = new DbQuery("INSERT INTO users (username, password, coins, elo, wins, losses, name, bio, image) " +
                "VALUES (@username, @password, @coins, @elo, @wins, @losses, @name, @bio, @image) RETURNING id");

            dbHandler.AddParameterWithValue("username", DbType.String, newUser.Username);
            dbHandler.AddParameterWithValue("password", DbType.String, newUser.Password);
            dbHandler.AddParameterWithValue("coins", DbType.Int32, 20);
            dbHandler.AddParameterWithValue("elo", DbType.Int32, 100);
            dbHandler.AddParameterWithValue("wins", DbType.Int32, 0);
            dbHandler.AddParameterWithValue("losses", DbType.Int32, 0);
            dbHandler.AddParameterWithValue("name", DbType.String, " "); //placeholders to avoid null
            dbHandler.AddParameterWithValue("bio", DbType.String, " ");
            dbHandler.AddParameterWithValue("image", DbType.String, " ");

            int id = (int)(dbHandler.ExecuteScalar() ?? 0);
            if (id != 0) return Response.CreateResponse("201", "Created", "", "application/json");

            return "???";
        }

        public string GetUserData(string username, string authToken, bool authNeeded)
        {
            string authorizedUser = SessionHandler.GetUsernameByToken(authToken);
            if (!string.Equals(username, authorizedUser) && authNeeded) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            DbQuery dbHandler = new DbQuery(@"SELECT * FROM users WHERE username = @username");

            dbHandler.AddParameterWithValue("username", DbType.String, username);
            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                if (reader.Read())
                {
                    UserData newUserData = new UserData()
                    {
                        Name = reader.GetString(7),
                        Bio = reader.GetString(8),
                        Image = reader.GetString(9)
                    };
                    return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(newUserData), "application/json");
                }
            }
            if (!authNeeded) return null;
            return Response.CreateResponse("404", "Not Found", "", "application/json");
        }

        public string UpdateUser(string username, string requestBody, string authToken)
        {
            UserData newUserData = new UserData();
            try
            {
                newUserData = JsonConvert.DeserializeObject<UserData>(requestBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string getUserResponse = GetUserData(username, authToken, true);

            if (string.Equals(getUserResponse.Substring(0, getUserResponse.IndexOf(Environment.NewLine)), "HTTP/1.1 401 Unauthorised"))//checks if getUser returned 401
            {
                return getUserResponse;
            }

            if (getUserResponse == null) return Response.CreateResponse("404", "Not Found", "", "application/json");

            DbQuery dbHandler = new DbQuery("UPDATE users SET name = @displayname, bio = @bio, image = @image WHERE username = @username");

            dbHandler.AddParameterWithValue("username", DbType.String, username);
            dbHandler.AddParameterWithValue("displayname", DbType.String, newUserData.Name);
            dbHandler.AddParameterWithValue("bio", DbType.String, newUserData.Bio);
            dbHandler.AddParameterWithValue("image", DbType.String, newUserData.Image);

            dbHandler.ExecuteNonQuery();
            return Response.CreateResponse("200", "OK", "", "application/json");
        }

        public string GetUserStats(string authToken, bool getAll)
        {
            string authorizedUser = SessionHandler.GetUsernameByToken(authToken);
            if (string.IsNullOrEmpty(authorizedUser) && !getAll) return Response.CreateResponse("401", "Unauthorised", "", "application/json");
            DbQuery dbHandler = new DbQuery(getAll ? @"SELECT name, elo, wins, losses FROM users;"
                : @"SELECT name, elo, wins, losses FROM users WHERE username = @username;");

            if (!getAll) dbHandler.AddParameterWithValue("username", DbType.String, authorizedUser);
            List<UserStats> userStats = new List<UserStats>();

            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                while (reader.Read())
                {
                    UserStats newUserStats = new UserStats()
                    {
                        Name = reader.GetString(0),
                        EloValue = reader.GetInt32(1),
                        Wins = reader.GetInt32(2),
                        Losses = reader.GetInt32(3)
                    };
                    userStats.Add(newUserStats);
                }
                if (userStats.Count != 0) return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(getAll ? userStats : userStats[0]), "application/json");
            }
            return Response.CreateResponse("404", "Not Found", "", "application/json");
        }
    }
}
