using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Exceeding
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Ustavka { get; set; }
        public int SummTime { get; set; }
        public double Prev { get; set; }

        public Exceeding()
        {

        }

        public Exceeding(int id, int analogSignalId, double ustavka, int summTime, double prev)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            Ustavka = ustavka;
            SummTime = summTime;
            Prev = prev;
        }

        public Exceeding(int analogSignalId, double ustavka, int summTime, double prev)
        {
            AnalogSignalId = analogSignalId;
            Ustavka = ustavka;
            SummTime = summTime;
            Prev = prev;
        }
    }
}
