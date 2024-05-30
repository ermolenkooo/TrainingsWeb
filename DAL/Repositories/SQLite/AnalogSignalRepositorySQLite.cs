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
    public class AnalogSignalRepositorySQLite : IAnalogSignalRepository
    {
        private SqliteConnection _connection;

        public AnalogSignalRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public List<AnalogSignal> GetList(int trainingId, int type)
        {
            List<AnalogSignal> ans = new List<AnalogSignal>();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "Select id, name, type, mark, exitId, exitName, func, result, baseNum from AnalogSignal where trainingId = $trainingId and signalGroup = $signalGroup";
            com.Parameters.Add("$trainingId", SqliteType.Integer).Value = trainingId;
            com.Parameters.Add("$signalGroup", SqliteType.Integer).Value = type;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                string name = Convert.ToString(reader[1]);
                string objType = Convert.ToString(reader[2]);
                string mark = Convert.ToString(reader[3]);
                int exitId = Convert.ToInt32(reader[4]);
                string exitName = Convert.ToString(reader[5]);
                string? func = Convert.IsDBNull(reader[6]) ? null : Convert.ToString(reader[6]);
                string? result = Convert.IsDBNull(reader[7]) ? null : Convert.ToString(reader[7]);
                int baseNum = Convert.ToInt32(reader[8]);
                ans.Add(new AnalogSignal(id, name, trainingId, objType, mark, exitId, exitName, func, result, type, baseNum));
            }
            return ans;
        }
    }
}
