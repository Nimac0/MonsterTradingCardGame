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

namespace MonsterTradingCardGame
{
    internal class DbHandler
    {
        private IDbCommand command;

        private static string connectionstring = "Host=localhost;Username=nimaco;Password=mtcgSwen;Database=mydb;Include Error Detail=true";
        
        public DbHandler(string commandtext)
        {
            this.command = ConnectToDb();
            this.command.CommandText = commandtext;
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
            var parameter = this.command.CreateParameter();
            parameter.DbType = type;
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            this.command.Parameters.Add(parameter);
        }

        public object? ExecuteScalar() {
            return command.ExecuteScalar();
        }

        public int ExecuteNonQuery()
        {
            return command.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            return command.ExecuteReader();
        }

    }
}
