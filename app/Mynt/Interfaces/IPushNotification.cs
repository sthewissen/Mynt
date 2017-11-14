using System;
using System.Threading.Tasks;

namespace Mynt.Interfaces
{
    public interface IPushNotification
    {
        Task<bool> RegisterDevice(string deviceToken);
    }
}
