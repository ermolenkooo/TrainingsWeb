using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class LogicVariableModel
    {
        public int Id { get; set; }
        public string ObjType { get; set; }
        public string Mark { get; set; }
        public int ExitId { get; set; }
        public string ExitName { get; set; }
        public int BaseNum { get; set; }
        public bool IsChecked { get; set; } = false;

        public LogicVariableModel(LogicVariable l) 
        {
            Id = l.Id;
            ObjType = l.ObjType;
            Mark = l.Mark;
            ExitId = l.ExitId;
            ExitName = l.ExitName;
            BaseNum = l.BaseNum;
        }
    }
}
