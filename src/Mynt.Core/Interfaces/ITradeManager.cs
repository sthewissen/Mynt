using System.Threading.Tasks;

namespace Mynt.Core.Interfaces
{
    public interface ITradeManager
    {
        /// <summary>
        /// Checks if new trades can be started.
        /// </summary>
        /// <returns></returns>
        Task LookForNewTrades();

        Task UpdateExistingTrades();
    }
}