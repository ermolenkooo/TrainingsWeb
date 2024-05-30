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
    public class ExceedingRepositorySQLite : IParameterRepository<Exceeding>
    {
        private SqliteConnection _connection;

        public ExceedingRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public Exceeding GetItem(int signalId)
        {
            Exceeding ans = new Exceeding();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, ustavka, summTime, prev FROM Exceeding where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new Exceeding(Convert.ToInt32(reader[0]), signalId, Convert.ToDouble(reader[1]), Convert.ToInt32(reader[2]), Convert.ToDouble(reader[3]));
            return ans;
        }
    }
}
