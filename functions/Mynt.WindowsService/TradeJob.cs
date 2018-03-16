﻿using Mynt.Core;
using Mynt.Core.Bittrex;
using Mynt.Core.NotificationManagers;
using Mynt.Core.Strategies;
using Mynt.Core.TradeManagers;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mynt.Core.Binance;

namespace Mynt.WindowsService
{
    [DisallowConcurrentExecution]
    public class TradeJob : IJob
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual Task Execute(IJobExecutionContext context)
        {
            log.Info($"Ready to shuffle your g0ldz!");

            var settings = new Constants();

            // Call the Bittrex Trade manager with the strategy of our choosing.
            var manager = new GenericTradeManager(new BittrexApi(settings), new BigThree(), null, (a) => log.Info(a), settings);

            // Call the process method to start processing the current situation.
            manager.CheckStrategySignals().GetAwaiter().GetResult();

            return Task.FromResult(true);
        }
    }
}
