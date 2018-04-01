using System.Threading.Tasks;

namespace Mynt.Core.TradeManagers
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