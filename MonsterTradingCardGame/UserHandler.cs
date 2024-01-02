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
                if (GetUserData(username, authToken, true) == null) return Response.CreateResponse("404", "Not Found", "", "application/json");
            }
            if ((GetUserData(newUser.Username, authToken, false) != null) && (username != newUser.Username)) return Response.CreateResponse("409", "Conflict", "", "application/json");

            DbHandler dbHandler = new DbHandler(isNew ? "INSERT INTO users (username, password, coins, elo) " +
                "VALUES (@username, @password, @coins, @elo) RETURNING id"
                : "UPDATE users SET username = @username, password = @password, coins = @coins, elo = @elo WHERE username = @oldusername");
                    
            dbHandler.AddParameterWithValue("username", DbType.String, newUser.Username);
            dbHandler.AddParameterWithValue("password", DbType.String, newUser.Password);
            dbHandler.AddParameterWithValue("coins", DbType.Int32, 20);
            dbHandler.AddParameterWithValue("elo", DbType.Int32, 100); // TODO add wins and losses (also to db)
            if(!isNew) dbHandler.AddParameterWithValue("oldusername", DbType.String, username);

            if (isNew)
            {
                int id = (int)(dbHandler.ExecuteScalar() ?? 0);
                if (id != 0) return Response.CreateResponse("201", "Created", "", "application/json");
            }
            if(!isNew)
            {
                dbHandler.ExecuteNonQuery();
                return Response.CreateResponse("200", "OK", "", "application/json");
            }

            return "???";
        }

        public string GetUserData(string username, string authToken, bool authNeeded) // TODO: add join for cards, check if user is authorized
        {
            string authorizedUser = SessionHandler.getUsernameByToken(authToken);
            if (!string.Equals(username, authorizedUser) && authNeeded) return Response.CreateResponse("401", "Unauthorized", "", "application/json");
            DbHandler dbHandler = new DbHandler(@"SELECT * FROM users WHERE username = @username");

            dbHandler.AddParameterWithValue("username", DbType.String, username);
            using (IDataReader reader = dbHandler.ExecuteReader())
            {
                if (reader.Read())
                {
                    CardHandler cardHandler = new CardHandler();
                    User newUser = new User()
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        Coins = reader.GetInt32(3),
                        EloValue = reader.GetInt32(4),
                        CardStack = cardHandler.GetCards(authToken)
                    };
                    return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(newUser), "application/json");
                }
            }

            return Response.CreateResponse("404", "Not Found", "", "application/json");
        }
    }
}
