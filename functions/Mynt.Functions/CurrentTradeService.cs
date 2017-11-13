using Mynt.Core;
using Mynt.Core.Api.Bittrex;
using Mynt.Core.Managers;
using Mynt.Core.Models;
using Mynt.Functions.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Mynt.Functions
{
    public static class CurrentTradeService
    {
        [FunctionName("CurrentTradeService")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "trades")]HttpRequestMessage req, TraceWriter log)
        {
            var tradeTable = await ConnectionManager.GetTableConnection(Constants.OrderTableName, Constants.IsDryRunning);

            var currentTrades = tradeTable.CreateQuery<Trade>().Where(x => x.IsOpen).ToList();
            currentTrades = currentTrades.OrderBy(x => x.Market).ToList();

            var api = new BittrexApi();
            List<TradeDto> trades = new List<TradeDto>();

            foreach (var trade in currentTrades)
            {
                var currentRate = await api.GetTicker(trade.Market);

                trades.Add(new TradeDto
                {
                    Market = trade.Market,
                    CloseRate = trade.CloseRate,
                    OpenRate = trade.OpenRate,
                    CloseProfit = trade.CloseProfitPercentage,
                    IsOpen = trade.IsOpen,
                    CloseDate = trade.CloseDate,
                    StakeAmount = trade.StakeAmount,
                    OpenDate = trade.OpenDate,
                    Quantity = trade.Quantity,
                    CurrentRate = currentRate.Ask,
                    Uuid = trade.RowKey
                });
            }

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK, trades);
        }
    }
}
