using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace DAL.Repositories.SQLite
{
    public class TrainingRepositorySQLite : ITrainingRepository
    {
        private SqliteConnection _connection;

        public TrainingRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public Training GetItem(int id)
        {
            Training ans = new Training();
            SqliteCommand selectTraining = _connection.CreateCommand();
            selectTraining.CommandText = "Select name, description, startMarka, mark, startDateTime from Training where id = " + id.ToString();
            SqliteDataReader reader = selectTraining.ExecuteReader();
            while (reader.Read())
            {
                string name = Convert.ToString(reader[0]);
                string description = Convert.ToString(reader[1]);
                string startMarka = Convert.ToString(reader[2]);
                int mark = Convert.ToInt32(reader[3]);
                DateTime? startDateTime = Convert.IsDBNull(reader[4]) ? null : new DateTime(Convert.ToInt64(reader[4]));
                ans = new Training(id, name, description, startMarka, mark, startDateTime);
            }
            return ans;
        }

        public List<Training> GetList()
        {
            List<Training> ans = new List<Training>();
            SqliteCommand selectAllTrainings = _connection.CreateCommand();
            selectAllTrainings.CommandText = "Select id, name, description, startMarka, mark, startDateTime from Training";
            SqliteDataReader reader = selectAllTrainings.ExecuteReader();
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                string name = Convert.ToString(reader[1]);
                string description = Convert.ToString(reader[2]);
                string startMarka = Convert.ToString(reader[3]);
                int mark = Convert.ToInt32(reader[4]);
                DateTime? startDateTime = Convert.IsDBNull(reader[5]) ? null : new DateTime(Convert.ToInt64(reader[5]));
                ans.Add(new Training(id, name, description, startMarka, mark, startDateTime));
            }
            return ans;
        }
    }
}
