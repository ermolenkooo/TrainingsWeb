using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DoubleDiscretSignal
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int LogicVariableId1 { get; set; }
        public int LogicVariableId2 { get; set; }
        public string Name { get; set; }

        public DoubleDiscretSignal(int id, int trainingId, int logicVariableId1, int logicVariableId2, string name)
        {
            Id = id;
            TrainingId = trainingId;
            LogicVariableId1 = logicVariableId1;
            LogicVariableId2 = logicVariableId2;
            Name = name;
        }

        public DoubleDiscretSignal(int trainingId, int logicVariableId1, int logicVariableId2, string name)
        {
            TrainingId = trainingId;
            LogicVariableId1 = logicVariableId1;
            LogicVariableId2 = logicVariableId2;
            Name = name;
        }
    }
}
