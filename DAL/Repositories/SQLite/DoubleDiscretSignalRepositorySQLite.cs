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
    public class DoubleDiscretSignalRepositorySQLite : IDoubleDiscretSignalRepository
    {
        private SqliteConnection _connection;

        public DoubleDiscretSignalRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public List<DoubleDiscretSignal> GetList(int trainingId)
        {
            List<DoubleDiscretSignal> ans = new List<DoubleDiscretSignal>();
            SqliteCommand selectAllDiscrets = _connection.CreateCommand();
            selectAllDiscrets.CommandText = "Select id, logicVariableId1, logicVariableId2, name from DoubleDiscretSignal where trainingId = $trainingId";
            selectAllDiscrets.Parameters.Add("$trainingId", SqliteType.Integer).Value = trainingId;
            SqliteDataReader reader = selectAllDiscrets.ExecuteReader();
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                int lv1 = Convert.ToInt32(reader[1]);
                int lv2 = Convert.ToInt32(reader[2]);
                string name = Convert.ToString(reader[3]);

                var s = new DoubleDiscretSignal(id, trainingId, lv1, lv2, name);
                ans.Add(s);
            }

            return ans;
        }
    }
}
