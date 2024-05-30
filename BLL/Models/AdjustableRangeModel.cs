using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class AdjustableRangeModel
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

        public AdjustableRangeModel(AdjustableRange a)
        {
            Id = a.Id;
            AnalogSignalId = a.AnalogSignalId;
            Type = a.Type;
            Mark = a.Mark;
            ExitId = a.ExitId;
            ExitName = a.ExitName;
            Left = a.Left;
            Right = a.Right;
            BaseNum = a.BaseNum;
        }
    }
}
