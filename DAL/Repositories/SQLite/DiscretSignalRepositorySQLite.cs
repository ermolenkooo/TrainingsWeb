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
    public class DiscretSignalRepositorySQLite : IDiscretSignalRepository
    {
        private SqliteConnection _connection;

        public DiscretSignalRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public List<DiscretSignal> GetList(int trainingId)
        {
            List<DiscretSignal> ans = new List<DiscretSignal>();
            SqliteCommand selectAllDiscrets = _connection.CreateCommand();
            selectAllDiscrets.CommandText = "Select id, trainingId, logicVariableId, name from DiscretSignal where trainingId = $trainingId";
            selectAllDiscrets.Parameters.Add("$trainingId", SqliteType.Integer).Value = trainingId;
            SqliteDataReader reader = selectAllDiscrets.ExecuteReader();
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                int logicVariableId = Convert.ToInt32(reader[2]);
                string name = Convert.ToString(reader[3]);
                ans.Add(new DiscretSignal(id, trainingId, logicVariableId, name));
            }
            return ans;
        }
    }
}
