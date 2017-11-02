using System.Threading.Tasks;

namespace Mynt.Core.Interfaces
{
    public interface ITradeManager
    {
        /// <summary>
        /// Queries the persistence layer for open trades and 
        /// handles them, otherwise a new trade is created.
        /// </summary>
        /// <returns></returns>
        Task Process();
    }
}