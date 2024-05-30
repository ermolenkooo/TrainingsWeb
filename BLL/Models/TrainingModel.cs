using DAL.Entities;

namespace BLL.Models
{
    public class TrainingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StartMarka { get; set; }
        public int? Mark { get; set; }
        public DateTime? StartDateTime { get; set; }

        public TrainingModel() { }

        public TrainingModel(Training t)
        {
            Id = t.Id;
            Name = t.Name;
            Description = t.Description;
            StartMarka = t.StartMarka;
            Mark = t.Mark;
            StartDateTime = t.StartDateTime;
        }
    }
}
