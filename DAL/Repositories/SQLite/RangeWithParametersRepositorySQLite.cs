using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.SQLite
{
    public class RangeWithParametersRepositorySQLite : IParameterRepository<RangeWithParameters>
    {
        private SqliteConnection _connection;

        public RangeWithParametersRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public RangeWithParameters GetItem(int signalId)
        {
            RangeWithParameters ans = new RangeWithParameters();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, type, mark, exitId, exitName, \"left\", right1, right2, paramVal1, paramVal2, baseNum FROM RangeWithParameters where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new RangeWithParameters(Convert.ToInt32(reader[0]), signalId, reader[1].ToString(), reader[2].ToString(), Convert.ToInt32(reader[3]), reader[4].ToString(), Convert.ToDouble(reader[5]), Convert.ToDouble(reader[6]), Convert.ToDouble(reader[7]), Convert.ToDouble(reader[8]), Convert.ToDouble(reader[9]), Convert.ToInt32(reader[10]));

            return ans;
        }
    }
}
