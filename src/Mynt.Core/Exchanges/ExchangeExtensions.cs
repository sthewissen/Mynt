using System;

namespace Mynt.Core.Exchanges
{
    public static class ExchangeExtensions
    {
        public static Enums.OrderStatus ToOrderStatus(this ExchangeSharp.ExchangeAPIOrderResult input)
        {
            switch (input)
            {
                case ExchangeSharp.ExchangeAPIOrderResult.Canceled:
                    return Enums.OrderStatus.Canceled;
                case ExchangeSharp.ExchangeAPIOrderResult.Error:
                    return Enums.OrderStatus.Error;
                case ExchangeSharp.ExchangeAPIOrderResult.Filled:
                    return Enums.OrderStatus.Filled;
                case ExchangeSharp.ExchangeAPIOrderResult.FilledPartially:
                    return Enums.OrderStatus.PartiallyFilled;
                case ExchangeSharp.ExchangeAPIOrderResult.Pending:
                    return Enums.OrderStatus.New;
                case ExchangeSharp.ExchangeAPIOrderResult.Unknown:
                    return Enums.OrderStatus.Unknown;
            }

            throw new ArgumentException($"{input} is an unknown OrderStatus");
        }
    }
}
