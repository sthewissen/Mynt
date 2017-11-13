using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Mynt.Core;
using Mynt.Core.Managers;
using Mynt.Core.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Mynt.Functions.Dto;

namespace Mynt.Functions
{
    public static class TradeHistoryService
    {
        [FunctionName("TradeHistoryService")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "history")]HttpRequestMessage req, TraceWriter log)
        {
            var tradeTable = await ConnectionManager.GetTableConnection(Constants.OrderTableName, Constants.IsDryRunning);
            var balanceTable = await ConnectionManager.GetTableConnection(Constants.BalanceTableName, Constants.IsDryRunning);

            var tradeHistory = tradeTable.CreateQuery<Trade>().Where(x => !x.IsOpen).AsQueryable().Take(30).ToList();
            tradeHistory = tradeHistory.OrderByDescending(x => x.OpenDate).ToList();

            var totalBalance = balanceTable.CreateQuery<Balance>().Where(x => x.PartitionKey == "BALANCE" && x.RowKey == "TOTAL").FirstOrDefault();
            var dayBalance = balanceTable.CreateQuery<Balance>().Where(x => x.PartitionKey == "BALANCE" && x.RowKey == DateTime.UtcNow.AddDays(-1).ToString("yyyyMMdd")).FirstOrDefault();

            var hist = new TradeHistoryDto()
            {
                Trades = tradeHistory.Select(x => new TradeDto()
                {
                    CloseDate = x.CloseDate,
                    OpenDate = x.OpenDate,
                    Market = x.Market,
                    CloseRate = x.CloseRate,
                    CloseProfitPercentage = x.CloseProfitPercentage,
                    CloseProfit = x.CloseProfit,
                    OpenRate = x.OpenRate,
                    Quantity = x.Quantity,
                    StakeAmount = x.StakeAmount,
                    Uuid = x.RowKey
                }).ToList(),
                TotalProfit = totalBalance?.Profit ?? 0,
                OverallBalance = totalBalance?.TotalBalance ?? 0,
                TodaysProfit = dayBalance?.Profit ?? 0,
            };

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK, hist);
        }
    }
}
