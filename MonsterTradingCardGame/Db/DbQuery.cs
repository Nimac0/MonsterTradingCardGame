using Microsoft.VisualBasic;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Db
{
    public class DbQuery : IDatabase
    {
        private IDbCommand command;

        private static string connectionstring = "Host=localhost;Username=nimaco;Password=mtcgSwen;Database=mydb;Include Error Detail=true";



        public DbQuery() {

        }
        
        public IDatabase NewCommand(string commandtext)
        {
            command = ConnectToDb();
            command.CommandText = commandtext;

            return this;
        }

        public IDbCommand ConnectToDb()
        {
            IDbConnection connection = new NpgsqlConnection(connectionstring);
            IDbCommand command = connection.CreateCommand();
            connection.Open();
            return command;
        }

        public void AddParameterWithValue(string parameterName, DbType type, object value)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = type;
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        public object? ExecuteScalar()
        {
            var executeScalar = command.ExecuteScalar();
            command.Connection.Close();
            return executeScalar;
        }

        public int ExecuteNonQuery()
        {
            int nonQuery = command.ExecuteNonQuery();
            command.Connection.Close();
            return nonQuery;
        }

        public IDataReader ExecuteReader()
        {
            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

    }
}
