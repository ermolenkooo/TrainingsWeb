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
    public class OperationWithConditionRepositorySQLite : IOperationWithConditionRepository
    {
        private SqliteConnection _connection;

        public OperationWithConditionRepositorySQLite(SqliteConnection connection)
        {
            _connection = connection;
        }

        public List<OperationWithCondition> GetList(int trainingId)
        {
            List<OperationWithCondition> ans = new List<OperationWithCondition>();
            using (SqliteCommand com = _connection.CreateCommand())
            {
                com.CommandText = "Select id, trainingId, type, mark, exitId, exitName, valueToWrite, conditionType, conditionMark, conditionExitId, conditionExitName, name, timePause, base1Num, base2Num from OperationWithCondition where trainingId = $trainingId";
                com.Parameters.Add("$trainingId", SqliteType.Integer).Value = trainingId;
                SqliteDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader[0]);
                    string type = Convert.ToString(reader[2]);
                    string mark = Convert.ToString(reader[3]);
                    int exitId = Convert.ToInt32(reader[4]);
                    string exitName = Convert.ToString(reader[5]);
                    string valueToWrite = Convert.ToString(reader[6]);
                    string conditionType = Convert.ToString(reader[7]);
                    string conditionMark = Convert.ToString(reader[8]);
                    int conditionExitId = Convert.ToInt32(reader[9]);
                    string conditionExitName = Convert.ToString(reader[10]);
                    string name = reader[11].ToString();
                    TimeSpan timePause = new TimeSpan(Convert.ToInt64(reader[12]));
                    int base1Num = Convert.ToInt32(reader[13]);
                    int base2Num = Convert.ToInt32(reader[14]);

                    ans.Add(new OperationWithCondition(id, name, trainingId, type, mark, exitId, exitName, valueToWrite, conditionType, conditionMark, conditionExitId, conditionExitName, timePause, base1Num, base2Num));
                }
            }
            return ans;
        }
    }
}
