using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class OperationWithCondition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TrainingId { get; set; }
        public TimeSpan TimePause { get; set; }
        public string ObjType { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public string ValueToWrite { get; set; }
        public string ConditionType { get; set; }
        public string ConditionMark { get; set; }
        public int ConditionExitId { get; set; }
        public string ConditionExitName { get; set; }
        public int Base1Num { get; set; }
        public int Base2Num { get; set; }

        public OperationWithCondition()
        {

        }

        public OperationWithCondition(int id, string name, int trainingId, string objType, string mark, int exitId, string exitName, string valueToWrite, string conditionType, string conditionMark, int conditionExitId, string conditionExitName, TimeSpan timePause, int base1Num, int base2Num)
        {
            Id = id;
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            ConditionType = conditionType;
            ConditionMark = conditionMark;
            ConditionExitId = conditionExitId;
            ConditionExitName = conditionExitName;
            TimePause = timePause;
            Base1Num = base1Num;
            Base2Num = base2Num;
        }

        public OperationWithCondition(string name, int trainingId, string objType, string mark, int exitId, string exitName, string valueToWrite, string conditionType, string conditionMark, int conditionExitId, string conditionExitName, TimeSpan timePause, int base1Num, int base2Num)
        {
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            ConditionType = conditionType;
            ConditionMark = conditionMark;
            ConditionExitId = conditionExitId;
            ConditionExitName = conditionExitName;
            TimePause = timePause;
            Base1Num = base1Num;
            Base2Num = base2Num;
        }

        public OperationWithCondition(int id, string name, int trainingId, string objType, string mark, int exitId, string exitName, string valueToWrite, string conditionType, string conditionMark, int conditionExitId, string conditionExitName, long timePause, int base1Num, int base2Num)
        {
            Id = id;
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            ConditionType = conditionType;
            ConditionMark = conditionMark;
            ConditionExitId = conditionExitId;
            ConditionExitName = conditionExitName;
            TimePause = new TimeSpan(timePause);
            Base1Num = base1Num;
            Base2Num = base2Num;
        }

        public OperationWithCondition(string name, int trainingId, string objType, string mark, int exitId, string exitName, string valueToWrite, string conditionType, string conditionMark, int conditionExitId, string conditionExitName, long timePause, int base1Num, int base2Num)
        {
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            ValueToWrite = valueToWrite;
            ConditionType = conditionType;
            ConditionMark = conditionMark;
            ConditionExitId = conditionExitId;
            ConditionExitName = conditionExitName;
            TimePause = new TimeSpan(timePause);
            Base1Num = base1Num;
            Base2Num = base2Num;
        }
    }
}
