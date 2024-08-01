using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class DiscretFromAnalogSignalModel
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
        public TimeSpan DeltaT { get; set; } = new TimeSpan(0);
        public bool IsChecked { get; set; } = false;
        public bool[] Tags { get; set; }

        public DiscretFromAnalogSignalModel() { }

        public DiscretFromAnalogSignalModel(DiscretFromAnalogSignal d)
        {
            Id = d.Id;
            Name = d.Name;
            TrainingId = d.TrainingId;
            ObjType = d.ObjType;
            Mark = d.Mark;
            ExitId = d.ExitId;
            ExitName = d.ExitName;
            Sign = d.Sign;
            Const = d.Const;
            IsInGroup = d.IsInGroup;
            BaseNum = d.BaseNum;
            Tags = new bool[5];
        }
    }
}
