using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class TimeBorders
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

        public TimeBorders() { }

        public TimeBorders(int id, int signalType, int signalId, long t1, long? t2, long? t3, long? t4, int score1, int? score2, int? score3, int? score4, int? score5)
        {
            Id = id;
            SignalType = signalType;
            SignalId = signalId;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
            Score1 = score1;
            Score2 = score2;
            Score3 = score3;
            Score4 = score4;
            Score5 = score5;
        }

        public TimeBorders(int signalType, int signalId, long t1, long? t2, long? t3, long? t4, int score1, int? score2, int? score3, int? score4, int? score5)
        {
            SignalType = signalType;
            SignalId = signalId;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
            Score1 = score1;
            Score2 = score2;
            Score3 = score3;
            Score4 = score4;
            Score5 = score5;
        }
    }
}
