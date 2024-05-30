using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class TimeInIntervalModel
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Bottom { get; set; }
        public double Top { get; set; }
        public string Sign { get; set; }
        public TimeSpan Ustavka { get; set; }
        public int Score { get; set; }

        public TimeInIntervalModel(TimeInInterval t)
        {
            Id = t.Id;
            AnalogSignalId = t.AnalogSignalId;
            Bottom = t.Bottom;
            Top = t.Top;
            Sign = t.Sign;
            Ustavka = t.Ustavka;
            Score = t.Score;
        }
    }
}
