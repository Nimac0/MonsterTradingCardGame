using Microsoft.VisualBasic;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class DbHandler
    {
        public static string connectionstring = "Host=localhost;Username=nimaco;Password=mtcgSwen;Database=mydb;Include Error Detail=true";

        public static void AddParameterWithValue(IDbCommand command, string parameterName, DbType type, object value)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = type;
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

    }
}
