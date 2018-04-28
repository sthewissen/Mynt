using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Interfaces
{
    public interface INotificationManager
    {
        Task<bool> SendNotification(string message);
        Task<bool> SendTemplatedNotification(string template, params object[] parameters);
    }
}
