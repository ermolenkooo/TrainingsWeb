using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class ExitToTheCorridorModel
    {
        public int Id { get; set; }
        public int AnalogSignalId { get; set; }
        public double Bottom { get; set; }
        public double Top { get; set; }
        public int Score { get; set; }

        public ExitToTheCorridorModel(ExitToTheCorridor e)
        {
            Id = e.Id;
            AnalogSignalId = e.AnalogSignalId;
            Bottom = e.Bottom;
            Top = e.Top;
            Score = e.Score;
        }
    }
}
