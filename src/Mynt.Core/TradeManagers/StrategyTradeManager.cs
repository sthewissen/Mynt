using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly DateTime _currentRunTime;
		private List<Trade> _currentTrades = new List<Trade>();
		private List<Trader> _availableTraders = new List<Trader>();
		private List<Trade> _tradesToSave = new List<Trade>();
		private List<Trader> _tradersToSave = new List<Trader>();

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

			_currentRunTime = DateTime.UtcNow;

			if (_api == null) throw new ArgumentException("Invalid exchange provided...");
			if (_strategy == null) throw new ArgumentException("Invalid strategy provided...");
			if (_dataStore == null) throw new ArgumentException("Invalid data store provided...");
			if (_settings == null) throw new ArgumentException("Invalid settings provided...");
			if (_logger == null) throw new ArgumentException("Invalid logger provided...");
		}

		#region initalization logic

        /// <summary>
        /// Initialize the traders.
        /// </summary>
        /// <returns>The initialize.</returns>
        /// <param name="initTraders">If set to <c>true</c> init traders.</param>
		async Task Initialize(bool initTraders = false)
		{
			// First initialize our data store, because this is used intesively here.
			await _dataStore.InitializeAsync();

			// Create traders if there are none...         
			if (initTraders)
			{
				var currentTraders = await _dataStore.GetTradersAsync();

				_logger.LogInformation("Currently have {CurrentTraders} traders out of {AllTraders}...", currentTraders.Count, _settings.MaxNumberOfConcurrentTrades);

				// Create our trader records if they're wrong.
				if (currentTraders.Count < _settings.MaxNumberOfConcurrentTrades)
					await CreateTraders(currentTraders.Count);
				else if (currentTraders.Count > _settings.MaxNumberOfConcurrentTrades)
					await ArchiveTraders(currentTraders);

				// TODO: What if stake amount was changed, we need to update our traders too...
			}

            // Get our active trades so we can use them later
			_currentTrades = await _dataStore.GetActiveTradesAsync();
		}

        /// <summary>
        /// Archives the traders.
        /// </summary>
        /// <returns>The traders.</returns>
        /// <param name="currentTraders">Current traders.</param>
		async Task ArchiveTraders(List<Trader> currentTraders)
		{
			// We need to archive some traders because we have more traders than our setting indicates.
			var amountToArchive = currentTraders.Count - _settings.MaxNumberOfConcurrentTrades;
			var closedTraders = 0;

			foreach (var item in currentTraders)
			{
				// If we've archived what we need, we can stop.
				if (closedTraders >= amountToArchive)
					break;

				// This trader is not busy, it can be closed.
				if (!item.IsBusy && !item.IsArchived)
				{
					item.IsArchived = true;
					closedTraders += 1;
				}
			}

			await _dataStore.SaveTradersAsync(currentTraders);
		}

        /// <summary>
        /// Creates the traders.
        /// </summary>
        /// <returns>The traders.</returns>
        /// <param name="currentAmount">Current amount.</param>
		async Task CreateTraders(int currentAmount)
		{
			var traders = new List<Trader>();

			for (var i = 0 + currentAmount; i < _settings.MaxNumberOfConcurrentTrades; i++)
			{
				var newTrader = new Trader()
				{
					Identifier = $"{Guid.NewGuid().ToString().Split('-').FirstOrDefault()}",
					CurrentBalance = _settings.AmountToInvestPerTrader,
					IsBusy = false,
					LastUpdated = DateTime.UtcNow,
					StakeAmount = _settings.AmountToInvestPerTrader,
				};

				traders.Add(newTrader);
			}

			if (traders.Count > 0)
			{
				await _dataStore.SaveTradersAsync(traders);
			}
		}

		#endregion

        /// <summary>
        /// Buy new things!
        /// </summary>
        /// <returns>The buy.</returns>
		public async Task Buy()
		{
			await Initialize(true);

			// Check if there are any traders available.
			_availableTraders = await _dataStore.GetAvailableTradersAsync();

			if (_availableTraders.Count > 0)
			{
				// Check the markets for opportunities
				var markets = await GetTradeMarkets();

				foreach (var market in markets)
				{
					// TODO: Check if we still have free traders.
					if (_availableTraders.Count > 0)
					{
						// Check if this market has a buy signal.
						var signal = await GetStrategySignal(market.MarketName);

						if (signal != null && signal.TradeAdvice == TradeAdvice.Buy)
						{
							// Buy it!
							await CreateTrade(_availableTraders.First(), signal);

							// Since we created a trade for it, this trader is no longer available.
							_availableTraders.RemoveAt(0);
						}
					}
					else
					{
						// Stop this if we don't have any traders available anymore.
						break;
					}
				}
			}

			// Store our changes to the database. By doing these at the end we can do all our changes in one (or well... 2) swift batches.
			await _dataStore.SaveTradesAsync(_tradesToSave);
			await _dataStore.SaveTradersAsync(_tradersToSave);
		}

        /// <summary>
        /// Sell all the things!
        /// </summary>
        /// <returns>The sell.</returns>
		public async Task Sell()
		{
			await Initialize();

			// Check our current trades if they're nearing sell conditions
		}

		#region trade management

		/// <summary>
		/// Creates the trade.
		/// </summary>
		/// <returns>The trade.</returns>
		/// <param name="availableTrader">Available trader.</param>
		/// <param name="buySignal">Buy signal.</param>
		async Task CreateTrade(Trader availableTrader, TradeSignal buySignal)
		{
			var exchangeQuoteBalance = _isPaperTrading ? null : await _api.GetBalance(buySignal.QuoteCurrency);
			var currentQuoteBalance = _isPaperTrading ? 9999 : exchangeQuoteBalance?.Available;

			// Do we even have enough funds to invest?
			if (currentQuoteBalance < availableTrader.CurrentBalance)
			{
				_logger.LogWarning("Insufficient funds ({Available}) to perform a {MarketName} trade. Skipping this trade.", currentQuoteBalance, buySignal.MarketName);
				return;
			}

			var order = await CreateBuyOrder(availableTrader, buySignal.MarketName, buySignal.SignalCandle);

			// We found a trade and have set it all up!
			if (order != null)
			{
				// Save the order.
				_tradesToSave.Add(order);

				// Send a notification that we found something suitable
				_logger.LogInformation("New trade signal {market}...", order.Market);

				// Update the trader to busy
				availableTrader.LastUpdated = DateTime.UtcNow;
				availableTrader.IsBusy = true;

				// Save the new trader state.
				_tradersToSave.Add(availableTrader);
			}
		}

		/// <summary>
		/// Creates the buy order.
		/// </summary>
		/// <returns>The buy order.</returns>
		/// <param name="availableTrader">Available trader.</param>
		/// <param name="pair">Pair.</param>
		/// <param name="signalCandle">Signal candle.</param>
		private async Task<Trade> CreateBuyOrder(Trader availableTrader, string pair, Candle signalCandle)
		{
			// Take the amount to invest per trader OR the current balance for this trader.
			var btcToSpend = 0.0m;

			if (availableTrader.CurrentBalance < _settings.AmountToInvestPerTrader || _settings.ProfitStrategy == ProfitType.Reinvest)
				btcToSpend = availableTrader.CurrentBalance;
			else
				btcToSpend = _settings.AmountToInvestPerTrader;

			// The amount here is an indication and will probably not be precisely what you get.
			var ticker = await _api.GetTicker(pair);
			var openRate = GetTargetBid(ticker, signalCandle);
			var amount = btcToSpend / openRate;

			// Get the order ID, this is the most important because we need this to check
			// up on our trade. We update the data below later when the final data is present.
			var orderId = _isPaperTrading ? CreateFakeOrderId() : await _api.Buy(pair, amount, openRate);

			await SendNotification($"Buying #{pair} with limit {openRate:0.00000000} BTC ({amount:0.0000} units).");

			var trade = new Trade()
			{
				TraderId = availableTrader.Identifier,
				Market = pair,
				StakeAmount = btcToSpend,
				OpenRate = openRate,
				OpenDate = DateTime.UtcNow,
				Quantity = amount,
				OpenOrderId = orderId,
				BuyOrderId = orderId,
				IsOpen = true,
				IsBuying = true,
				StrategyUsed = _strategy.Name,
				SellType = SellType.None,
			};

			if (_settings.PlaceFirstStopAtSignalCandleLow)
			{
				trade.StopLossRate = signalCandle.Low;
				_logger.LogInformation("Automatic stop set at signal candle low {Low}", signalCandle.Low.ToString("0.00000000"));
			}

			return trade;
		}

		#endregion

		#region helper methods

		/// <summary>
		/// Gets the possible trade markets.
		/// </summary>
		/// <returns>The trade markets.</returns>
		async Task<List<MarketSummary>> GetTradeMarkets()
		{
			// Retrieve all the current markets on the exchange
			var markets = await _api.GetMarketSummaries(_settings.QuoteCurrency);

			markets = markets.Where(x => (x.Volume > _settings.MinimumAmountOfVolume || // Only get markets matching our volume.
									 _settings.AlwaysTradeList.Contains(x.CurrencyPair.BaseCurrency)) && // Also get markets that we always want to trade.
									_settings.QuoteCurrency.ToUpper() == x.CurrencyPair.QuoteCurrency.ToUpper()).ToList();// Only get markets matching our quote currency.

			// If there are items on the only trade list remove the rest.
			if (_settings.OnlyTradeList.Count > 0)
				markets = markets.Where(m => _settings.OnlyTradeList.Any(c => c == m.CurrencyPair.BaseCurrency)).ToList();

			// Remove existing trades from the list to check.
			foreach (var trade in _currentTrades)
				markets.RemoveAll(x => x.MarketName == trade.Market);

			// Remove items that are on our blacklist.
			foreach (var market in _settings.MarketBlackList)
				markets.RemoveAll(x => x.CurrencyPair.BaseCurrency == market);

			// Return our list of available markets, ordered by volume.
			return markets.Distinct().OrderByDescending(x => x.Volume).ToList();
		}

		/// <summary>
		/// Gets the strategy signal for the given market.
		/// </summary>
		/// <returns>The strategy signal.</returns>
		/// <param name="market">Market.</param>
		async Task<TradeSignal> GetStrategySignal(string market)
		{
			try
			{
				_logger.LogInformation("Checking market {Market}...", market);

				// Gather the current dates for the candle data we want to use.
				var minimumDate = _strategy.GetMinimumDateTime(_currentRunTime);
				var candleDate = _strategy.GetCurrentCandleDateTime(_currentRunTime);
				var strategySignalDate = _strategy.GetSignalDate(_currentRunTime);

				// Get the candles needed to calculate our strategy.
				var candles = await _api.GetTickerHistory(market, _strategy.IdealPeriod, minimumDate, candleDate);

				// We eliminate all candles that aren't needed for the dataset incl. the last one (if it's the current running candle).
				candles = candles.Where(x => x.Timestamp >= minimumDate && x.Timestamp < candleDate).ToList();

				// Not enough candles to perform what we need to do.
				if (candles.Count < _strategy.MinimumAmountOfCandles)
				{
					_logger.LogWarning("Not enough candle data for {Market}...", market);
					return new TradeSignal { TradeAdvice = TradeAdvice.Hold, MarketName = market };
				}

				// Get the date for the last candle.
				var signalDate = candles[candles.Count - 1].Timestamp;

				// This is an outdated candle...
				if (signalDate < strategySignalDate)
				{
					_logger.LogInformation("Outdated candle for {Market}...", market);
					return null;
				}

				// This calculates an advice for what to do now.
				var advice = _strategy.Forecast(candles);

				return new TradeSignal
				{
					TradeAdvice = advice,
					MarketName = market,
					SignalCandle = _strategy.GetSignalCandle(candles)
				};
			}
			catch (Exception ex)
			{
				// Couldn't get a buy signal for this market, no problem. Let's skip it.
				_logger.LogError(ex, "Couldn't get buy signal for {Market}...", market);
				return null;
			}
		}

		/// <summary>
		/// Gets the target bid.
		/// </summary>
		/// <returns>The target bid.</returns>
		/// <param name="tick">Tick.</param>
		/// <param name="signalCandle">Signal candle.</param>
		decimal GetTargetBid(Ticker tick, Candle signalCandle)
		{
			switch (_settings.BuyInPriceStrategy)
			{
				case BuyInPriceStrategy.AskLastBalance:
					// If the ask is below the last, we can get it on the cheap.
					if (tick.Ask < tick.Last)
						return tick.Ask;

					return Math.Round(tick.Ask + _settings.AskLastBalance * (tick.Last - tick.Ask), 8);

				case BuyInPriceStrategy.SignalCandleClose:
					return signalCandle.Close;

				case BuyInPriceStrategy.MatchCurrentBid:
					return tick.Bid;

				default:
					return Math.Round(tick.Bid * (1 - _settings.BuyInPricePercentage), 8);
			}
		}

		#endregion

		#region notification methods

		/// <summary>
		/// Sends the notification.
		/// </summary>
		/// <returns>The notification.</returns>
		/// <param name="message">Message.</param>
		async Task SendNotification(string message) => await _notification?.SendNotification(message);

		#endregion

		#region paper trade methods

		/// <summary>
		/// Creates the fake order identifier.
		/// </summary>
		/// <returns>The fake order identifier.</returns>
		string CreateFakeOrderId() => Guid.NewGuid().ToString().Replace("-", string.Empty);

		#endregion
	}
}
