using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class OperationWithConditionModel
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
        public bool IsChecked { get; set; } = false;

        public OperationWithConditionModel(OperationWithCondition o)
        {
            Id = o.Id;
            Name = o.Name;
            TrainingId = o.TrainingId;
            TimePause = o.TimePause;
            ObjType = o.ObjType;
            Mark = o.Mark;
            ExitId = o.ExitId;
            ExitName = o.ExitName;
            ValueToWrite = o.ValueToWrite;
            ConditionType = o.ConditionType;
            ConditionMark = o.ConditionMark;
            ConditionExitId = o.ConditionExitId;
            ConditionExitName = o.ConditionExitName;
            Base1Num = o.Base1Num;
            Base2Num = o.Base2Num;
        }
    }
}
