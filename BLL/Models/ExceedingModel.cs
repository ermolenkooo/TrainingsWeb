using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class ExceedingModel
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Ustavka { get; set; }
        public int SummTime { get; set; }
        public double Prev { get; set; }

        public ExceedingModel(Exceeding e)
        {
            Id = e.Id;
            AnalogSignalId = e.AnalogSignalId;
            Ustavka = e.Ustavka;
            SummTime = e.SummTime;
            Prev = e.Prev;
        }
    }
}
