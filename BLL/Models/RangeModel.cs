using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class RangeModel
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public int AbsValues { get; set; }

        public RangeModel(DAL.Entities.Range r)
        {
            Id = r.Id;
            AnalogSignalId = r.AnalogSignalId;
            Left = r.Left;
            Right = r.Right;
            AbsValues = r.AbsValues;
        }
    }
}
