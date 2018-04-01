using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mynt.Core.TradeManagers;

namespace Mynt.AspNetCore.Host.Hosting
{
    internal class MyntHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<MyntHostedService> _logger;
        // TODO options
        private readonly TimeSpan _span;
        private readonly TimeSpan _delayLookup;
        private readonly TimeSpan _delayUpdate;
        private readonly ITradeManager _tradeManager;

        private Timer _timerLookup;
        private Timer _timerUpdate;

        public MyntHostedService(ILogger<MyntHostedService> logger, TimeSpan span, TimeSpan delayLookup, TimeSpan delayUpdate, ITradeManager tradeManager)
        {
            _logger = logger;
            _span = span;
            _delayLookup = delayLookup;
            _delayUpdate = delayUpdate;
            _tradeManager = tradeManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mynt service is starting.");

            _timerLookup = new Timer(OnLookup, null, _delayLookup, _span);
            _timerUpdate = new Timer(OnUpdate, null, _delayUpdate, _span);

            return Task.CompletedTask;
        }

        private async void OnLookup(object state)
        {
            _logger.LogInformation("Mynt service is looking for new trades.");
            await _tradeManager.LookForNewTrades();
        }

        private async void OnUpdate(object state)
        {
            _logger.LogInformation("Mynt service is updating trades.");
            await _tradeManager.UpdateExistingTrades();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mynt service is stopping.");

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