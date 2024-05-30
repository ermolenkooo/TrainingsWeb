using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Range
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public int AbsValues { get; set; }

        public Range()
        {

        }

        public Range(int id, int analogSignalId, double left, double right, int absValues)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            Left = left;
            Right = right;
            AbsValues = absValues;
        }

        public Range(int analogSignalId, double left, double right, int absValues)
        {
            AnalogSignalId = analogSignalId;
            Left = left;
            Right = right;
            AbsValues = absValues;
        }
    }
}
