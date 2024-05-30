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
    public class AdjustableRangeRepositorySQLite : IParameterRepository<AdjustableRange>
    {
        private SqliteConnection _connection;

        public AdjustableRangeRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public AdjustableRange GetItem(int signalId)
        {
            AdjustableRange ans = new AdjustableRange();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, type, mark, exitId, exitName, \"left\", \"right\", baseNum FROM AdjustableRange where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new AdjustableRange(Convert.ToInt32(reader[0]), signalId, reader[1].ToString(), reader[2].ToString(), Convert.ToInt32(reader[3]), reader[4].ToString(), Convert.ToDouble(reader[5]), Convert.ToDouble(reader[6]), Convert.ToInt32(reader[7]));

            return ans;
        }
    }
}
