using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ILogicVariableRepository
    {
        LogicVariable GetItem(int id);
        List<LogicVariable> GetItemsStartSub(int id);
        List<LogicVariable> GetItemsEndSub(int id);
    }
}
