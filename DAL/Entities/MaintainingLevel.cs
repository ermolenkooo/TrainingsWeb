using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class MaintainingLevel
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double OtlBorder { get; set; }
        public double Ustavka { get; set; }
        public double NeydBorder { get; set; }

        public MaintainingLevel()
        {

        }

        public MaintainingLevel(int id, int analogSignalId, double otlBorder, double ustavka, double neydBorder)
        {
            Id = id;
            AnalogSignalId = analogSignalId;
            OtlBorder = otlBorder;
            Ustavka = ustavka;
            NeydBorder = neydBorder;
        }

        public MaintainingLevel(int analogSignalId, double otlBorder, double ustavka, double neydBorder)
        {
            AnalogSignalId = analogSignalId;
            OtlBorder = otlBorder;
            Ustavka = ustavka;
            NeydBorder = neydBorder;
        }
    }
}
