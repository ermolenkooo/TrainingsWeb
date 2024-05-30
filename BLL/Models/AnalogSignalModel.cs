using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class AnalogSignalModel
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

        public AnalogSignalModel(AnalogSignal a)
        {
            Id = a.Id;
            Name = a.Name;
            TrainingId = a.TrainingId;
            ObjType = a.ObjType;
            Mark = a.Mark;
            ExitId = a.ExitId;
            ExitName = a.ExitName;
            Func = a.Func;
            Result = a.Result;
            SignalGroup = a.SignalGroup;
            BaseNum = a.BaseNum;
        }
    }
}
