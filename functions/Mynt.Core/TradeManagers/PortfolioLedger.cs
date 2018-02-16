using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Mynt.Core.Api;
using Mynt.Core.Enums;
using Mynt.Core.Models;

namespace Mynt.Core
{
    public class PortfolioLedger
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly double exchangeFee = AppSettings.Get<double>("ExchangeFee");

        private readonly IExchangeApi api;

        private IEnumerable<CreditPosition> creditPositions;

        private IList<(string, string)> openOrders = new List<(string, string)>();

        public PortfolioLedger(IExchangeApi api, IEnumerable<CurrencyPair> pairs, double initialBudget) :
            this(api, pairs.Select(_ => (_, initialBudget)))
        {
        }

        public PortfolioLedger(IExchangeApi api, IEnumerable<(CurrencyPair, double)> entries)
        {
            this.api = api;
            var task = CreateCreditPositions(api, entries);
            task.Wait();
            creditPositions = task.Result;
        }

        public CreditPosition GetCreditPosition(string symbol)
        {
            return creditPositions.SingleOrDefault(_ => _.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<CreditPosition> GetCreditPositions()
        {
            return creditPositions;
        }

        public void AddOpenOrder(string orderId, string market)
        {
            openOrders.Add((orderId, market));
            log.Info($"Added order with order ID {orderId} (market {market})");
        }

        public async Task Update()
        {
            log.Info("Updating the portfolio ledger");

            for (int index = openOrders.Count - 1; index >= 0; index--)
            {
                var item = openOrders[index];
                var order = await api.GetOrder(item.Item1, item.Item2);

                if (order == null)
                {
                    openOrders.Remove(item);
                    log.Info($"Removed order with order ID {item.Item1} (market {item.Item2}) because the order cannot be found");
                    break;
                }

                switch (order.Status)
                {
                    case OrderStatus.Canceled:
                    case OrderStatus.Expired:
                    case OrderStatus.PendingCancel:
                    case OrderStatus.Rejected:
                    case OrderStatus.Filled:
                        // Update the credit position and remove the order.
                        UpdateCreditPosition(order);
                        openOrders.Remove(item);
                        log.Info($"Removed order with order ID {item.Item1} (market {item.Item2}) because its status is {order.Status}");
                        break;
                    default:
                        // Do nothing.
                        break;
                }
            }

            log.Info("Updating the portfolio ledger is done");
        }

        private void UpdateCreditPosition(Order order)
        {
            var creditPosition = creditPositions.SingleOrDefault(_ => _.Symbol == order.Symbol);
            if (creditPosition != null)
            {
                if (order.Side == OrderSide.Buy)
                {
                    creditPosition.RegisterBuy(order.ExecutedQuantity, order.Price);
                }
                else
                {
                    creditPosition.RegisterSell(order.ExecutedQuantity, order.Price);
                }
            }
        }

        private async static Task<IEnumerable<CreditPosition>> CreateCreditPositions(IExchangeApi api, IEnumerable<(CurrencyPair, double)> entries)
        {
            IList<CreditPosition> creditPositions = new List<CreditPosition>();
            foreach (var entry in entries)
            {
                var symbol = $"{entry.Item1.BaseCurrency}{entry.Item1.QuoteCurrency}";
                var ticker = await api.GetTicker(symbol);
                var balance = await api.GetBalance(entry.Item1.BaseCurrency);
                var btcCredit = entry.Item2 - balance.Balance * ticker.Last;
                var creditPosition = new CreditPosition(symbol, exchangeFee, balance.Balance, ticker.Last, btcCredit);

                creditPositions.Add(creditPosition);
                log.Info($"Create credit position for {entry.Item1.BaseCurrency}/{entry.Item1.QuoteCurrency} (balance {balance.Balance * ticker.Last:#0.##########} BTC). BTC credit: {btcCredit:#0.##########}");
            }

            return creditPositions;
        }
    }
}
