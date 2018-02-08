using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.DataAccess.Interfaces
{
    public interface IDataStorage
    {
        void Save<T>(string parentKey, IEnumerable<T> items);
    }
}
