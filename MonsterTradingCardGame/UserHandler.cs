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
        public string RegisterUser(string requestBody, string authToken)
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

            using IDbConnection connection = new NpgsqlConnection(DbHandler.connectionstring);
            using IDbCommand command = connection.CreateCommand();
            connection.Open();

            if (GetUserData(newUser.Username, authToken, false) != null) return "user already exists";

            command.CommandText = "INSERT INTO users (username, password, coins, elo) " +
                "VALUES (@username, @password, @coins, @elo) RETURNING id";
            DbHandler.AddParameterWithValue(command, "username", DbType.String, newUser.Username);
            DbHandler.AddParameterWithValue(command, "password", DbType.String, newUser.Password);
            DbHandler.AddParameterWithValue(command, "coins", DbType.Int32, 20);
            DbHandler.AddParameterWithValue(command, "elo", DbType.Int32, 100);

            int id = (int)(command.ExecuteScalar() ?? 0);
            if (id != 0) return "registered user";

            return "???";
        }

        public string GetUserData(string username, string authToken, bool authNeeded) // TODO: add join for cards, check if user is authorized
        {
            if (!string.Equals(authToken.TrimEnd('\r', '\n'), username + "-mtcgToken") && authNeeded)
                return "user not authorized";
            using IDbConnection connection = new NpgsqlConnection(DbHandler.connectionstring);
            using IDbCommand command = connection.CreateCommand();
            connection.Open();

            command.CommandText = @"SELECT * FROM users WHERE username = @username";
            DbHandler.AddParameterWithValue(command, "username", DbType.String, username);
            using (IDataReader reader = command.ExecuteReader())
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

            return null;
        }

        public string UpdateUserData(string username)
        {
            return "updated user";
            //TODO db request to update userdata 
        }
    }
}
