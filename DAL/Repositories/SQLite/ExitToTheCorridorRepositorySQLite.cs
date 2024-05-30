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
    public class ExitToTheCorridorRepositorySQLite : IParameterRepository<ExitToTheCorridor>
    {
        private SqliteConnection _connection;

        public ExitToTheCorridorRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public ExitToTheCorridor GetItem(int signalId)
        {
            ExitToTheCorridor ans = new ExitToTheCorridor();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, \"bottom\", \"top\", score FROM ExitToTheCorridor where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new ExitToTheCorridor(Convert.ToInt32(reader[0]), signalId, Convert.ToDouble(reader[1]), Convert.ToDouble(reader[2]), Convert.ToInt32(reader[3]));
            return ans;
        }
    }
}
