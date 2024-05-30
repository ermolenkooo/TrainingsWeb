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
    public class OperationRepositorySQLite : IOperationRepository
    {
        private SqliteConnection _connection;

        public OperationRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public List<Operation> GetList(int trainingId)
        {
            List<Operation> ans = new List<Operation>();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, timePause, type, mark, exitId, exitName, valueToWrite, baseNum FROM Operation WHERE trainingId = " + trainingId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans.Add(new Operation(Convert.ToInt32(reader[0]), trainingId, Convert.ToInt64(reader[1]), Convert.ToString(reader[2]),
                    Convert.ToString(reader[3]), Convert.ToInt32(reader[4]), Convert.ToString(reader[5]), Convert.ToString(reader[6]), Convert.ToInt32(reader[7])));
            return ans;
        }
    }
}
