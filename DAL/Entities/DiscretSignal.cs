using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DiscretSignal
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int LogicVariableId { get; set; }
        public string Name { get; set; }

        public DiscretSignal(int id, int trainingId, int logicVariableId, string name)
        {
            Id = id;
            TrainingId = trainingId;
            LogicVariableId = logicVariableId;
            Name = name;
        }

        public DiscretSignal(int trainingId, int logicVariableId, string name)
        {
            TrainingId = trainingId;
            LogicVariableId = logicVariableId;
            Name = name;
        }
    }
}
