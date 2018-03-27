using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Common.Framework.Singleton
{
    /// <summary>
    /// Represents a Singleton stored in SingletonManager
    /// </summary>
    public class SingletonBase<T>
        where T : ISingletonManageable, new()
    {
        public static T Instance
        {
            get { return SingletonManager.Instance.Get<T>(); }
        }
    }
}
