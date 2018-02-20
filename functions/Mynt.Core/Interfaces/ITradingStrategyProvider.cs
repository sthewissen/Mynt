using System.Collections.Generic;

namespace Mynt.Core.Interfaces
{
    public interface ITradingStrategyProvider
    {
        IEnumerable<ITradingStrategy> CreateStrategies();
    }
}
