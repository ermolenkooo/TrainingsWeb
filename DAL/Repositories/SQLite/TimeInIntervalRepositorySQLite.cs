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
    public class TimeInIntervalRepositorySQLite : IParameterRepository<TimeInInterval>
    {
        private SqliteConnection _connection;

        public TimeInIntervalRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public TimeInInterval GetItem(int signalId)
        {
            TimeInInterval ans = new TimeInInterval();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, \"bottom\", \"top\", sign, ustavka, score FROM TimeInInterval where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new TimeInInterval(Convert.ToInt32(reader[0]), signalId, Convert.ToDouble(reader[1]), Convert.ToDouble(reader[2]), reader[3].ToString(), Convert.ToInt64(reader[4]), Convert.ToInt32(reader[5]));
            return ans;
        }
    }
}
