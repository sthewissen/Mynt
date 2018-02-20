using System.ComponentModel;

namespace Mynt.Core.Enums
{
    public enum TradeAdvice
    {
        [Description("Strong Sell")]
        StrongSell = -2,
        Sell = -1,
        Hold = 0,
        Buy=1,
        [Description("Strong Buy")]
        StrongBuy =2
    }
}
