using System;
using System.Threading;
using System.Threading.Tasks;
using ExchangeSharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mynt.Core.Interfaces;
using Mynt.Core.TradeManagers;

namespace Mynt.AspNetCore.WindowsService.Hosting
{
    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private TimeSpan _span;
        private TimeSpan _delayLookup;
        private TimeSpan _delayUpdate;
        private readonly PaperTradeManager _tradeManager;

        private Timer _timerLookup;
        private Timer _timerUpdate;

        public TimedHostedService(ILogger logger, TimeSpan span, TimeSpan delayLookup, TimeSpan delayUpdate, PaperTradeManager tradeManager)
        {
            _logger = logger;
            _span = span;
            _delayLookup = delayLookup;
            _delayUpdate = delayUpdate;
            _tradeManager = tradeManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            _timerLookup = new Timer(OnLookup, null, _delayLookup, _span);
            _timerUpdate = new Timer(OnUpdate, null, _delayUpdate, _span);

            return Task.CompletedTask;
        }

        private async void OnLookup(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");
            await _tradeManager.LookForNewTrades();
        }

        private async void OnUpdate(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");
            await _tradeManager.UpdateExistingTrades();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timerLookup?.Change(Timeout.Infinite, 0);
            _timerUpdate?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timerLookup?.Dispose();
            _timerUpdate?.Dispose();
        }
    }
}