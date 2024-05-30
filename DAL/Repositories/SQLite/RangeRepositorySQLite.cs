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
    public class RangeRepositorySQLite : IParameterRepository<Entities.Range>
    {
        private SqliteConnection _connection;

        public RangeRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public Entities.Range GetItem(int signalId)
        {
            Entities.Range ans = new Entities.Range();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, \"left\", \"right\", absValues FROM Range where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new Entities.Range(Convert.ToInt32(reader[0]), signalId, Convert.ToDouble(reader[1]), Convert.ToDouble(reader[2]), Convert.ToInt32(reader[3]));

            return ans;
        }
    }
}
