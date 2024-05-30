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
    public class GroupOfDiscretSignalsRepositorySQLite : IGroupOfDiscretSignalsRepository
    {
        private SqliteConnection _connection;

        public GroupOfDiscretSignalsRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public List<GroupOfDiscretSignals> GetList(int trainingId)
        {
            List<GroupOfDiscretSignals> ans = new List<GroupOfDiscretSignals>();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, startSubGroupId, endSubGroupId, name FROM GroupOfDiscretSignals WHERE trainingId = " + trainingId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans.Add(new GroupOfDiscretSignals(Convert.ToInt32(reader[0]), trainingId, Convert.ToInt32(reader[1]), Convert.ToInt32(reader[2]), Convert.ToString(reader[3])));

            return ans;
        }
    }
}
