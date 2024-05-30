using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class LogicVariable
    {
        public int Id { get; set; }
        public string ObjType { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public int BaseNum { get; set; }

        public LogicVariable(int id, string objType, string mark, int exitId, string exitName, int baseNum)
        {
            Id = id;
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            BaseNum = baseNum;
        }

        public LogicVariable(string objType, string mark, int exitId, string exitName, int baseNum)
        {
            ObjType = objType;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            BaseNum = baseNum;
        }

        public LogicVariable() { }
    }
}
