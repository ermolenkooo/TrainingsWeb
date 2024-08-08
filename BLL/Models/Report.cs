using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class Report
    {
        public string FIO { get; set; }
        public string Position { get; set; }
        public DateTime Date { get; set; }
        public string TrainingName { get; set; }
        public string[] CriteriasWithMarks { get; set; }
        public string EndMark { get; set; }

        public Report()
        {

        }
    }
}
