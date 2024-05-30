using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class OperationModel
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
        public TimeSpan DeltaT { get; set; } = new TimeSpan(0);
        public bool IsChecked { get; set; } = false;

        public OperationModel(Operation o)
        {
            Id = o.Id;
            TrainingId = o.TrainingId;
            TimePause = o.TimePause;
            ObjType = o.ObjType;
            Mark = o.Mark;
            ExitId = o.ExitId;
            ExitName = o.ExitName;
            ValueToWrite = o.ValueToWrite;
            BaseNum = o.BaseNum;
        }
    }
}
