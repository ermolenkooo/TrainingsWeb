using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class GroupOfDiscretSignalsModel
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int StartSubGroupId { get; set; }
        public int EndSubGroupId { get; set; }
        public string Name { get; set; }
        public TimeSpan DeltaT { get; set; } = new TimeSpan(0);
        public bool IsChecked { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public List<LogicVariableModel> StartLogicVariables { get; set; } = new List<LogicVariableModel>();
        public List<DiscretFromAnalogSignalModel> StartSignals { get; set; } = new List<DiscretFromAnalogSignalModel>();
        public List<LogicVariableModel> EndLogicVariables { get; set; } = new List<LogicVariableModel>();
        public List<DiscretFromAnalogSignalModel> EndSignals { get; set; } = new List<DiscretFromAnalogSignalModel>();
        public bool[] Tags { get; set; }

        public GroupOfDiscretSignalsModel(GroupOfDiscretSignals g)
        {
            Id = g.Id;
            TrainingId = g.TrainingId;
            StartSubGroupId = g.StartSubGroupId;
            EndSubGroupId = g.EndSubGroupId;
            Name = g.Name;
            Tags = new bool[5];
        }
    }
}
