using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class DiscretSignalModel
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int LogicVariableId { get; set; }
        public string Name { get; set; }
        public TimeSpan DeltaT { get; set; }
        public bool IsChecked { get; set; } = false;

        public DiscretSignalModel(DiscretSignal d) 
        {
            Id = d.Id;
            TrainingId = d.TrainingId;
            LogicVariableId = d.LogicVariableId;
            Name = d.Name;
        }
    }
}
