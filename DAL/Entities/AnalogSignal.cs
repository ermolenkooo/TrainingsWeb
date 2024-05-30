using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class AnalogSignal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TrainingId { get; set; }
        public string ObjType { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public string? Func { get; set; }
        public string? Result { get; set; }
        public int SignalGroup { get; set; }
        public int BaseNum { get; set; }

        public AnalogSignal(int id, string name, int trainingId, string objType, string mark, int exitId, string exitName, string? func, string? result, int signalGroup, int baseNum)
        {
            Id = id;
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Func = func;
            Result = result;
            SignalGroup = signalGroup;
            BaseNum = baseNum;
        }

        public AnalogSignal(string name, int trainingId, string objType, string mark, int exitId, string exitName, string? func, string? result, int signalGroup, int baseNum)
        {
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Func = func;
            Result = result;
            SignalGroup = signalGroup;
            BaseNum = baseNum;
        }
    }
}
