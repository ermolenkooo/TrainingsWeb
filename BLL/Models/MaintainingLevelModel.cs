using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class MaintainingLevelModel
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double OtlBorder { get; set; }
        public double Ustavka { get; set; }
        public double NeydBorder { get; set; }

        public MaintainingLevelModel(MaintainingLevel m)
        {
            Id = m.Id;
            AnalogSignalId = m.AnalogSignalId;
            OtlBorder = m.OtlBorder;
            Ustavka = m.Ustavka;
            NeydBorder = m.NeydBorder;
        }
    }
}
