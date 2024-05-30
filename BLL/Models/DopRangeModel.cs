using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class DopRangeModel
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double OtlBorder { get; set; }
        public double XorBorder { get; set; }
        public double YdBorder { get; set; }
        public double NeydBorder { get; set; }

        public DopRangeModel(DopRange d)
        {
            Id = d.Id;
            AnalogSignalId = d.AnalogSignalId;
            OtlBorder = d.OtlBorder;
            XorBorder = d.XorBorder;
            YdBorder = d.YdBorder;
            NeydBorder = d.NeydBorder;
        }
    }
}
