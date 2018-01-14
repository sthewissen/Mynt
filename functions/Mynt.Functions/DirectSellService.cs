using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Mynt.Core;
using Mynt.Core.Bittrex;
using Mynt.Core.Managers;
using Mynt.Core.Models;
using Mynt.Core.NotificationManagers;
using Mynt.Functions.Dto;
using Newtonsoft.Json;

namespace Mynt.Functions
{
    public static class DirectSellService
    {
        [FunctionName("DirectSellService")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                //read json object from request body
                var content = req.Content;
                var jsonContent = content.ReadAsStringAsync().Result;
                var order = JsonConvert.DeserializeObject<TradeDto>(jsonContent);

                var tradeTable = await ConnectionManager.GetTableConnection(Constants.OrderTableName, Constants.IsDryRunning);
                var activeTrade = tradeTable.CreateQuery<Trade>().Where(x => x.RowKey == order.Uuid).FirstOrDefault();

                // Directly sell it off.
                var tradeManager = new BittrexTradeManager(null, new PushNotificationManager(), (a) => log.Info(a));
                await tradeManager.DirectSell(activeTrade);
    
                tradeTable.Execute(TableOperation.Replace(activeTrade));

                return req.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.OK, false);
            }
        }
    }
}
