using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class TimeBordersModel
    {
        public int Id { get; set; }
        public int SignalType { get; set; }
        public int SignalId { get; set; }
        public long T1 { get; set; }
        public long? T2 { get; set; }
        public long? T3 { get; set; }
        public long? T4 { get; set; }
        public int Score1 { get; set; }
        public int? Score2 { get; set; }
        public int? Score3 { get; set; }
        public int? Score4 { get; set; }
        public int? Score5 { get; set; }

        public TimeBordersModel(TimeBorders t) 
        { 
            Id = t.Id;
            SignalType = t.SignalType;
            SignalId = t.SignalId;
            T1 = t.T1;
            T2 = t.T2;
            T3 = t.T3;
            T4 = t.T4;
            Score1 = t.Score1;
            Score2 = t.Score2;
            Score3 = t.Score3;
            Score4 = t.Score4;
            Score5 = t.Score5;
        }
    }
}
