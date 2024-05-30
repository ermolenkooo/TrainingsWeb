using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Training
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StartMarka { get; set; }
        public int? Mark { get; set; }
        public DateTime? StartDateTime { get; set; }

        public Training() { }

        public Training(int id, string name, string description, string startMarka, int? mark, DateTime? startDateTime)
        {
            Id = id;
            Name = name;
            Description = description;
            StartMarka = startMarka;
            Mark = mark;
            StartDateTime = startDateTime;
        }

        public Training(string name, string description, string startMarka, int? mark, DateTime? startDateTime)
        {
            Name = name;
            Description = description;
            StartMarka = startMarka;
            Mark = mark;
            StartDateTime = startDateTime;
        }

        public Training(int id, string name, string description, string startMarka, int? mark, long startDateTime)
        {
            Id = id;
            Name = name;
            Description = description;
            StartMarka = startMarka;
            Mark = mark;
            StartDateTime = new DateTime(startDateTime);
        }

        public Training(string name, string description, string startMarka, int? mark, long startDateTime)
        {
            Name = name;
            Description = description;
            StartMarka = startMarka;
            Mark = mark;
            StartDateTime = new DateTime(startDateTime);
        }
    }
}
