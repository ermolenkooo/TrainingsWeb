using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ExitToTheCorridor
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Bottom { get; set; }
        public double Top { get; set; }
        public int Score { get; set; }

        public ExitToTheCorridor()
        {

        }

        public ExitToTheCorridor(int id, int analogSignalId, double bottom, double top, int score)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            Bottom = bottom;
            Top = top;
            Score = score;
        }

        public ExitToTheCorridor(int analogSignalId, double bottom, double top, int score)
        {
            AnalogSignalId = analogSignalId;
            Bottom = bottom;
            Top = top;
            Score = score;
        }
    }
}
