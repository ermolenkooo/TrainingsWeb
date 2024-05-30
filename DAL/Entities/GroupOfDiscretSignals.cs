using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class GroupOfDiscretSignals
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int StartSubGroupId { get; set; }
        public int EndSubGroupId { get; set; }
        public string Name { get; set; }

        public GroupOfDiscretSignals(int id, int trainingId, int startSubGroupId, int endSubGroupId, string name)
        {
            Id = id;
            TrainingId = trainingId;
            StartSubGroupId = startSubGroupId;
            EndSubGroupId = endSubGroupId;
            Name = name;
        }

        public GroupOfDiscretSignals(int trainingId, int startSubGroupId, int endSubGroupId, string name)
        {
            TrainingId = trainingId;
            StartSubGroupId = startSubGroupId;
            EndSubGroupId = endSubGroupId;
            Name = name;
        }
    }
}
