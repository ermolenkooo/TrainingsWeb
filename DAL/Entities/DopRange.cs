using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DopRange
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double OtlBorder { get; set; }
        public double XorBorder { get; set; }
        public double YdBorder { get; set; }
        public double NeydBorder { get; set; }

        public DopRange()
        {

        }

        public DopRange(int id, int analogSignalId, double otlBorder, double xorBorder, double ydBorder, double neydBorder)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            OtlBorder = otlBorder;
            XorBorder = xorBorder;
            YdBorder = ydBorder;
            NeydBorder = neydBorder;
        }

        public DopRange(int analogSignalId, double otlBorder, double xorBorder, double ydBorder, double neydBorder)
        {
            AnalogSignalId = analogSignalId;
            OtlBorder = otlBorder;
            XorBorder = xorBorder;
            YdBorder = ydBorder;
            NeydBorder = neydBorder;
        }
    }
}
