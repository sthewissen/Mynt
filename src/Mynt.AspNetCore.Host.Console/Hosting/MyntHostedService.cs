using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mynt.Core.Interfaces;

namespace Mynt.AspNetCore.Host.Hosting
{
    public class MyntHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<MyntHostedService> _logger;
        private readonly ITradeManager _tradeManager;
        private readonly IOptions<MyntHostedServiceOptions> _options;

        private CancellationTokenSource _cancellationTokenSource;

        public MyntHostedService(ITradeManager tradeManager, IOptions<MyntHostedServiceOptions> options, ILogger<MyntHostedService> logger)
        {
            _logger = logger;
            _tradeManager = tradeManager;
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mynt service is starting.");

            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(() => SpinUpNewCronJob(_options.Value.BuyTimer, OnBuy), cancellationToken);
            Task.Run(() => SpinUpNewCronJob(_options.Value.SellTimer, OnSell), cancellationToken);

            return Task.CompletedTask;
        }

        private async Task SpinUpNewCronJob(string cronTime, Action action)
        {
            try
            {
                var schedule = NCrontab.CrontabSchedule.Parse(cronTime);
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var now = DateTime.Now;
                        var span = schedule.GetNextOccurrence(now) - now;
                        await Task.Delay(span, _cancellationTokenSource.Token);
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            continue;
                        }

                        action();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while processing a timed job.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while starting a timed job.");
            }
        }

        private async void OnBuy()
        {
            _logger.LogInformation("Mynt service is looking for new trades.");
            await _tradeManager.LookForNewTrades();
        }

        private async void OnSell()
        {
            _logger.LogInformation("Mynt service is updating trades.");
            await _tradeManager.UpdateExistingTrades();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mynt service is stopping.");

            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}