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
    public class DopRangeRepositorySQLite : IParameterRepository<DopRange>
    {
        private SqliteConnection _connection;

        public DopRangeRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public DopRange GetItem(int signalId)
        {
            DopRange ans = new DopRange();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, otlBorder, xorBorder, ydBorder, neydBorder FROM DopRange where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new DopRange(Convert.ToInt32(reader[0]), signalId, Convert.ToDouble(reader[1]), Convert.ToDouble(reader[2]), Convert.ToDouble(reader[3]), Convert.ToDouble(reader[4]));
            return ans;
        }
    }
}
