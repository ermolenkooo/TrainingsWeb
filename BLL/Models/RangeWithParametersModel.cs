using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class RangeWithParametersModel
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

        public RangeWithParametersModel(RangeWithParameters r)
        {
            Id = r.Id;
            AnalogSignalId = r.AnalogSignalId;
            Type = r.Type;
            Mark = r.Mark;
            ExitId = r.ExitId;
            ExitName = r.ExitName;
            Left = r.Left;
            Right1 = r.Right1;
            Right2 = r.Right2;
            ParamVal1 = r.ParamVal1;
            ParamVal2 = r.ParamVal2;
            BaseNum = r.BaseNum;
        }
    }
}
