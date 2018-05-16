using System;
namespace Mynt.Core.Interfaces
{
    public interface INotificationTradingStrategy
    {
       string BuyMessage { get; }
       string SellMessage { get; }
    }
}
