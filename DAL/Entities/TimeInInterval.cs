using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class TimeInInterval
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Bottom { get; set; }
        public double Top { get; set; }
        public string Sign { get; set; }
        public TimeSpan Ustavka { get; set; }
        public int Score { get; set; }

        public TimeInInterval()
        {

        }

        public TimeInInterval(int id, int analogSignalId, double bottom, double top, string sign, TimeSpan ustavka, int score)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            Bottom = bottom;
            Top = top;
            Sign = sign;
            Ustavka = ustavka;
            Score = score;
        }

        public TimeInInterval(int analogSignalId, double bottom, double top, string sign, TimeSpan ustavka, int score)
        {
            AnalogSignalId = analogSignalId;
            Bottom = bottom;
            Top = top;
            Sign = sign;
            Ustavka = ustavka;
            Score = score;
        }

        public TimeInInterval(int id, int analogSignalId, double bottom, double top, string sign, long ustavka, int score)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            Bottom = bottom;
            Top = top;
            Sign = sign;
            Ustavka = new TimeSpan(ustavka);
            Score = score;
        }

        public TimeInInterval(int analogSignalId, double bottom, double top, string sign, long ustavka, int score)
        {
            AnalogSignalId = analogSignalId;
            Bottom = bottom;
            Top = top;
            Sign = sign;
            Ustavka = new TimeSpan(ustavka);
            Score = score;
        }
    }
}
