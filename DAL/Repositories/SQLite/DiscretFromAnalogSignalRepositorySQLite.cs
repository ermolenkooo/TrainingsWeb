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
    public class DiscretFromAnalogSignalRepositorySQLite : IDiscretFromAnalogSignalRepository
    {
        private SqliteConnection _connection;

        public DiscretFromAnalogSignalRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public DiscretFromAnalogSignal GetItem(int id)
        {
            DiscretFromAnalogSignal ans = new DiscretFromAnalogSignal();
            SqliteCommand selectAllDiscrets = _connection.CreateCommand();
            selectAllDiscrets.CommandText = "Select trainingId, name, objType, mark, exitId, exitName, sign, const, baseNum from DiscretFromAnalogSignal where id = $id";
            selectAllDiscrets.Parameters.Add("$id", SqliteType.Integer).Value = id;
            SqliteDataReader reader = selectAllDiscrets.ExecuteReader();
            while (reader.Read())
            {
                int trainingId = Convert.ToInt32(reader[0]);
                string name = Convert.ToString(reader[1]);
                string objType = Convert.ToString(reader[2]);
                string mark = Convert.ToString(reader[3]);
                int exitId = Convert.ToInt32(reader[4]);
                string exitName = Convert.ToString(reader[5]);
                string sign = Convert.ToString(reader[6]);
                float @const = Convert.ToSingle(reader[7]);
                int baseNum = Convert.ToInt32(reader[8]);
                ans = new DiscretFromAnalogSignal(id, name, trainingId, objType, mark, exitId, exitName, sign, @const, 0, baseNum);
            }
            return ans;
        }

        public List<DiscretFromAnalogSignal> GetItemsEndSub(int id)
        {
            var ans = new List<DiscretFromAnalogSignal>();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "Select discretId from EndSubGroupComparison where endSubGroupId = $endSubGroupId AND discretType = 1";
            com.Parameters.Add("$endSubGroupId", SqliteType.Integer).Value = id;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                ans.Add(GetItem(Convert.ToInt32(reader[0])));
            }
            return ans;
        }

        public List<DiscretFromAnalogSignal> GetItemsStartSub(int id)
        {
            var ans = new List<DiscretFromAnalogSignal>();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "Select discretId from StartSubGroupComparison where startSubGroupId = $startSubGroupId AND discretType = 1";
            com.Parameters.Add("$startSubGroupId", SqliteType.Integer).Value = id;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                ans.Add(GetItem(Convert.ToInt32(reader[0])));
            }
            return ans;
        }

        public List<DiscretFromAnalogSignal> GetList(int trainingId, int isInGroup)
        {
            List<DiscretFromAnalogSignal> ans = new List<DiscretFromAnalogSignal>();
            SqliteCommand selectAllDiscrets = _connection.CreateCommand();
            selectAllDiscrets.CommandText = "Select id, name, objType, mark, exitId, exitName, sign, const, baseNum from DiscretFromAnalogSignal where trainingId = $trainingId AND isInGroup = $isInGroup";
            selectAllDiscrets.Parameters.Add("$trainingId", SqliteType.Integer).Value = trainingId;
            selectAllDiscrets.Parameters.Add("$isInGroup", SqliteType.Integer).Value = isInGroup;
            SqliteDataReader reader = selectAllDiscrets.ExecuteReader();
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                string name = Convert.ToString(reader[1]);
                string objType = Convert.ToString(reader[2]);
                string mark = Convert.ToString(reader[3]);
                int exitId = Convert.ToInt32(reader[4]);
                string exitName = Convert.ToString(reader[5]);
                string sign = Convert.ToString(reader[6]);
                float @const = Convert.ToSingle(reader[7]);
                int baseNum = Convert.ToInt32(reader[8]);
                ans.Add(new DiscretFromAnalogSignal(id, name, trainingId, objType, mark, exitId, exitName, sign, @const, isInGroup, baseNum));
            }

            return ans;
        }
    }
}
