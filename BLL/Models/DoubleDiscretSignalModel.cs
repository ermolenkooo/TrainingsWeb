using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class DoubleDiscretSignalModel
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int LogicVariableId1 { get; set; }
        public int LogicVariableId2 { get; set; }
        public string Name { get; set; }
        public TimeSpan DeltaT { get; set; } = new TimeSpan(0);
        public bool IsChecked { get; set; } = false;
        public DateTime? StartDate { get; set; } = null;
        public bool[] Tags { get; set; }

        public DoubleDiscretSignalModel(DoubleDiscretSignal d)
        {
            Id = d.Id;
            TrainingId = d.TrainingId;
            LogicVariableId1 = d.LogicVariableId1;
            LogicVariableId2 = d.LogicVariableId2;
            Name = d.Name;
            Tags = new bool[5];
        }
    }
}
