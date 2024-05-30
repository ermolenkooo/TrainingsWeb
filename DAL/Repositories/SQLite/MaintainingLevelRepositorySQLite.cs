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
    public class MaintainingLevelRepositorySQLite : IParameterRepository<MaintainingLevel>
    {
        private SqliteConnection _connection;

        public MaintainingLevelRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public MaintainingLevel GetItem(int signalId)
        {
            MaintainingLevel ans = new MaintainingLevel();
            SqliteCommand com = _connection.CreateCommand();
            com.CommandText = "SELECT id, otlBorder, ustavka, neydBorder FROM MaintainingLevel where analogSignalId = $analogSignalId";
            com.Parameters.Add("$analogSignalId", SqliteType.Integer).Value = signalId;
            SqliteDataReader reader = com.ExecuteReader();
            while (reader.Read())
                ans = new MaintainingLevel(Convert.ToInt32(reader[0]), signalId, Convert.ToDouble(reader[1]), Convert.ToDouble(reader[2]), Convert.ToDouble(reader[3]));
            return ans;
        }
    }
}
