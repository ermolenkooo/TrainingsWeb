using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DiscretFromAnalogSignal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TrainingId { get; set; }
        public string ObjType { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public string Sign { get; set; }
        public double Const { get; set; }
        public int IsInGroup { get; set; }
        public int BaseNum { get; set; }

        public DiscretFromAnalogSignal() { }

        public DiscretFromAnalogSignal(int id, string name, int trainingId, string objType, string mark, int exitId, string exitName, string sign, double @const, int isInGroup, int baseNum)
        {
            Id = id;
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Sign = sign;
            Const = @const;
            IsInGroup = isInGroup;
            BaseNum = baseNum;
        }

        public DiscretFromAnalogSignal(string name, int trainingId, string objType, string mark, int exitId, string exitName, string sign, double @const, int isInGroup, int baseNum)
        {
            Name = name;
            TrainingId = trainingId;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Sign = sign;
            Const = @const;
            IsInGroup = isInGroup;
            BaseNum = baseNum;
        }
    }
}
