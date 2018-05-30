using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mynt.Core.Enums;
using Mynt.Core.Interfaces;
using Mynt.Core.Models;

namespace Mynt.Core.TradeManagers
{
    public class StrategyTradeManager
	{
		// These represent the things we will be using :)
		private readonly IExchangeApi _api;
        private readonly INotificationManager _notification;
        private readonly ITradingStrategy _strategy;
        private readonly ILogger _logger;
        private readonly IDataStore _dataStore;
        private readonly OrderBehavior _orderBehavior;
        private readonly TradeOptions _settings;
		private readonly bool _isPaperTrading;

		// Some variables for internal use.
		private List<Trade> _currentTrades;
		private List<Trader> _availableTraders;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mynt.Core.TradeManagers.StrategyTradeManager"/> class.
        /// </summary>
        /// <param name="api">API.</param>
        /// <param name="strategy">Strategy.</param>
        /// <param name="notificationManager">Notification manager.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="settings">Settings.</param>
        /// <param name="dataStore">Data store.</param>
        /// <param name="isPaperTrading">If set to <c>true</c> is paper trading.</param>
        /// <param name="paperTradeOrderBehavior">Paper trade order behavior.</param>
		public StrategyTradeManager(IExchangeApi api, ITradingStrategy strategy, INotificationManager notificationManager, ILogger logger, TradeOptions settings, IDataStore dataStore, bool isPaperTrading = true, OrderBehavior paperTradeOrderBehavior = OrderBehavior.AlwaysFill)
        {
            _api = api;
            _strategy = strategy;
            _logger = logger;
            _notification = notificationManager;
            _dataStore = dataStore;
			_isPaperTrading = isPaperTrading;
            _orderBehavior = paperTradeOrderBehavior;
            _settings = settings;

            if (_api == null) throw new ArgumentException("Invalid exchange provided...");
            if (_strategy == null) throw new ArgumentException("Invalid strategy provided...");
            if (_dataStore == null) throw new ArgumentException("Invalid data store provided...");
            if (_settings == null) throw new ArgumentException("Invalid settings provided...");
            if (_logger == null) throw new ArgumentException("Invalid logger provided...");
        }

        public async Task Buy()
		{
			// Check if there are any traders available.
		}

		public async Task Sell()
		{
			// Check our current trades if they're nearing sell conditions          
		}
    }
}
