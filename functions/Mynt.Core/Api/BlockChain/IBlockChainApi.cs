using System.Threading.Tasks;
using Mynt.Core.Api.BlockChain.Models;
using Refit;

namespace Mynt.Core.Api.BlockChain
{
    public interface IBlockChainApi
    {
        [Get("/en/ticker")]
        Task<Prices> GetEuroPrice();
    }
}
