using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IDiscretFromAnalogSignalRepository
    {
        List<DiscretFromAnalogSignal> GetList(int trainingId, int isInGroup);
        DiscretFromAnalogSignal GetItem(int id);
        List<DiscretFromAnalogSignal> GetItemsStartSub(int id);
        List<DiscretFromAnalogSignal> GetItemsEndSub(int id);
    }
}
