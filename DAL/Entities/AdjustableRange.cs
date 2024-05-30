using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class AdjustableRange
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public string Type { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public int BaseNum { get; set; }

        public AdjustableRange()
        {

        }

        public AdjustableRange(int id, int analogSignalId, string type, string mark, int exitId, string exitName, double left, double right, int baseNum)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            Type = type;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Left = left;
            Right = right;
            BaseNum = baseNum;
        }

        public AdjustableRange(int analogSignalId, string type, string mark, int exitId, string exitName, double left, double right, int baseNum)
        {
            AnalogSignalId = analogSignalId;
            Type = type;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Left = left;
            Right = right;
            BaseNum = baseNum;
        }
    }
}
