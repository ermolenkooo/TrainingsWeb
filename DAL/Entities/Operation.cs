using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Operation
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public TimeSpan TimePause { get; set; }
        public string ObjType { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public string ValueToWrite { get; set; }
        public int BaseNum { get; set; }

        public Operation(int id, int trainingId, TimeSpan timePause, string objType, string mark, int exitId, string exitName, string valueToWrite, int baseNum)
        {
            Id = id;
            TrainingId = trainingId;
            TimePause = timePause;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            BaseNum = baseNum;
        }

        public Operation(int trainingId, TimeSpan timePause, string objType, string mark, int exitId, string exitName, string valueToWrite, int baseNum)
        {
            TrainingId = trainingId;
            TimePause = timePause;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            BaseNum = baseNum;
        }

        public Operation(int id, int trainingId, long timePause, string objType, string mark, int exitId, string exitName, string valueToWrite, int baseNum)
        {
            Id = id;
            TrainingId = trainingId;
            TimePause = new TimeSpan(timePause);
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            BaseNum = baseNum;
        }

        public Operation(int trainingId, long timePause, string objType, string mark, int exitId, string exitName, string valueToWrite, int baseNum)
        {
            TrainingId = trainingId;
            TimePause = new TimeSpan(timePause);
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            BaseNum = baseNum;
        }
    }
}
