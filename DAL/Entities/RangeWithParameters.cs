using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class RangeWithParameters
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public string Type { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public double Left { get; set; }
        public double Right1 { get; set; }
        public double Right2 { get; set; }
        public double ParamVal1 { get; set; }
        public double ParamVal2 { get; set; }
        public int BaseNum { get; set; }

        public RangeWithParameters()
        {

        }

        public RangeWithParameters(int id, int analogSignalId, string type, string mark, int exitId, string exitName, double left, double right1, double right2, double paramVal1, double paramVal2, int baseNum)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            Type = type;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Left = left;
            Right1 = right1;
            Right2 = right2;
            ParamVal1 = paramVal1;
            ParamVal2 = paramVal2;
            BaseNum = baseNum;
        }

        public RangeWithParameters(int analogSignalId, string type, string mark, int exitId, string exitName, double left, double right1, double right2, double paramVal1, double paramVal2, int baseNum)
        {
            AnalogSignalId = analogSignalId;
            Type = type;
            Mark = mark;
            ExitId = exitId;
            ExitName = exitName;
            Left = left;
            Right1 = right1;
            Right2 = right2;
            ParamVal1 = paramVal1;
            ParamVal2 = paramVal2;
            BaseNum = baseNum;
        }
    }
}
