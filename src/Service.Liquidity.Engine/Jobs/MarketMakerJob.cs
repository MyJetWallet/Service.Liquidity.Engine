using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Domain.Services.Settings;

namespace Service.Liquidity.Engine.Jobs
{
    public class MarketMakerJob : IDisposable
    {
        private readonly IMarketMaker[] _marketMakers;
        private readonly IMarketMakerSettingsAccessor _settingsAccessor;
        private MyTaskTimer _timer;
        private bool _isFirstRun = true;

        public MarketMakerJob(ILogger<MarketMakerJob> logger, IMarketMaker[] marketMakers, IMarketMakerSettingsAccessor settingsAccessor)
        {
            _marketMakers = marketMakers;
            _settingsAccessor = settingsAccessor;
            _timer = new MyTaskTimer(typeof(MarketMakerJob), 
                TimeSpan.FromMilliseconds(settingsAccessor.GetMarketMakerSettings().MarketMakerRefreshIntervalMSec), 
                logger, DoTime);
        }

        private async Task DoTime()
        {
            if (_isFirstRun)
            {
                _isFirstRun = false;
                
                //todo: придумать как отвалижировать что все данные в кешах уже готовы и заменить слип тут
                await Task.Delay(10000);
            }

            foreach (var marketMaker in _marketMakers)
            {
                await marketMaker.RefreshOrders();
            }

            _timer.ChangeInterval(TimeSpan.FromMilliseconds(_settingsAccessor.GetMarketMakerSettings().MarketMakerRefreshIntervalMSec));
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}