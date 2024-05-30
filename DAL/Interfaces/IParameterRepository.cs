using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IParameterRepository<T> where T : class
    {
        T GetItem(int signalId);
    }
}
