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
    public class LogicVariableRepositorySQLite : ILogicVariableRepository
    {
        private SqliteConnection _connection;

        public LogicVariableRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public LogicVariable GetItem(int id)
        {
            var ans = new LogicVariable();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "Select type, mark, exitId, exitName, baseNum from LogicVariable WHERE id = " + id;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new LogicVariable(id, Convert.ToString(reader[0]), Convert.ToString(reader[1]), Convert.ToInt32(reader[2]), Convert.ToString(reader[3]), Convert.ToInt32(reader[4]));
            return ans;
        }

        public List<LogicVariable> GetItemsEndSub(int id)
        {
            var ans = new List<LogicVariable>();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "Select discretId from EndSubGroupComparison where endSubGroupId = $endSubGroupId AND discretType = 0";
            com.Parameters.Add("$endSubGroupId", SqliteType.Integer).Value = id;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                ans.Add(GetItem(Convert.ToInt32(reader[0])));
            }
            return ans;
        }

        public List<LogicVariable> GetItemsStartSub(int id)
        {
            var ans = new List<LogicVariable>();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "Select discretId from StartSubGroupComparison where startSubGroupId = $startSubGroupId AND discretType = 0";
            com.Parameters.Add("$startSubGroupId", SqliteType.Integer).Value = id;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                ans.Add(GetItem(Convert.ToInt32(reader[0])));
            }
            return ans;
        }
    }
}
