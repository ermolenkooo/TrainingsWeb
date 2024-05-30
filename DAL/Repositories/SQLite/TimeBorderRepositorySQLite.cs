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
    public class TimeBorderRepositorySQLite : ITimeBorderRepository
    {
        private SqliteConnection _connection;

        public TimeBorderRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public TimeBorders GetItem(int signalId, int type)
        {
            TimeBorders ans = new TimeBorders();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "Select id, t1, t2, t3, t4, score1, score2, score3, score4, score5 from TimeBorders where signalType = " + type + " AND signalId = " + signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                long t1 = Convert.ToInt64(reader[1]);
                long? t2 = Convert.IsDBNull(reader[2]) ? null : Convert.ToInt64(reader[2]);
                long? t3 = Convert.IsDBNull(reader[3]) ? null : Convert.ToInt64(reader[3]);
                long? t4 = Convert.IsDBNull(reader[4]) ? null : Convert.ToInt64(reader[4]);
                int score1 = Convert.ToInt32(reader[5]);
                int? score2 = Convert.IsDBNull(reader[6]) ? null : Convert.ToInt32(reader[6]);
                int? score3 = Convert.IsDBNull(reader[7]) ? null : Convert.ToInt32(reader[7]);
                int? score4 = Convert.IsDBNull(reader[8]) ? null : Convert.ToInt32(reader[8]);
                int? score5 = Convert.IsDBNull(reader[9]) ? null : Convert.ToInt32(reader[9]);
                ans = new TimeBorders(id, type, signalId, t1, t2, t3, t4, score1, score2, score3, score4, score5);
            }
            return ans;
        }
    }
}
