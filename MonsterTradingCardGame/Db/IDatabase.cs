using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Db
{
    public interface IDatabase
    {
        public DbQuery NewCommand(string commandtext);

        public IDbCommand ConnectToDb();

        public void AddParameterWithValue(string parameterName, DbType type, object value);
        
        public object? ExecuteScalar();

        public int ExecuteNonQuery();

        public IDataReader ExecuteReader();
    }
}
