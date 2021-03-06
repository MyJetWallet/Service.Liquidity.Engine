using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using Service.Liquidity.Engine.Domain.Services.Settings;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets
{
    public class ExternalBalanceCacheManager: IExternalBalanceCacheManager, IStartable, IDisposable
    {
        private readonly IExternalMarketManager _manager;
        private readonly IMarketMakerSettingsAccessor _settings;
        private readonly ILogger<ExternalBalanceCacheManager> _logger;
        private readonly Dictionary<string, Dictionary<string, ExchangeBalance>> _balances = new();
        private readonly Dictionary<string, Dictionary<string, ExchangeMarketInfo>> _markets = new();
        private readonly object _sync = new();
        private readonly MyTaskTimer _timer;

        public ExternalBalanceCacheManager(IExternalMarketManager manager, IMarketMakerSettingsAccessor settings, ILogger<ExternalBalanceCacheManager> logger)
        {
            _manager = manager;
            _settings = settings;
            _logger = logger;
            _timer = new MyTaskTimer(nameof(ExternalBalanceCacheManager),
                TimeSpan.FromMilliseconds(_settings.GetMarketMakerSettings().RefreshExternalBalanceIntervalMSec),
                logger,
                DoRefresh);
        }

        private async Task DoRefresh()
        {
            _timer.ChangeInterval(TimeSpan.FromMilliseconds(_settings.GetMarketMakerSettings().RefreshExternalBalanceIntervalMSec));

            await RefreshBalancesAndMarketInfo();
        }

        private async Task RefreshBalancesAndMarketInfo()
        {
            var rootActivity = MyTelemetry.StartActivity("Refresh external market data cache");

            foreach (var name in _manager.GetMarketNames())
            {
                var activity = MyTelemetry.StartActivity($"Refresh market {name}")?.AddTag("market", name);

                try
                {

                    var client = _manager.GetExternalMarketByName(name);

                    var resp = await client.GetBalancesAsync(null);

                    var infos = await client.GetMarketInfoListAsync(null);

                    lock (_sync)
                    {
                        if (!_balances.TryGetValue(name, out var balancesData))
                        {
                            balancesData = new Dictionary<string, ExchangeBalance>();
                            _balances[name] = balancesData;
                        }

                        foreach (var balance in resp.Balances ?? new List<ExchangeBalance>())
                        {
                            balancesData[balance.Symbol] = balance;
                        }


                        if (!_markets.TryGetValue(name, out var marketData))
                        {
                            marketData = new ();
                            _markets[name] = marketData;
                        }

                        foreach (var info in infos.Infos ?? new List<ExchangeMarketInfo>())
                        {
                            marketData[info.Market] = info;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot refresh external balances and instrument info for {marketName}", name);
                    ex.FailActivity();
                }
            }
        }

        public ExchangeBalance GetBalances(string marketName, string symbol)
        {
            lock (_sync)
            {
                if (!_balances.TryGetValue(marketName, out var balancesData) ||
                    !balancesData.TryGetValue(symbol, out var data))
                {
                    return new ExchangeBalance(){ Symbol = symbol, Balance = 0, Free = 0 };
                }

                return data;
            }
        }

        public List<ExchangeBalance> GetBalances(string marketName)
        {
            lock (_sync)
            {
                if (!_balances.TryGetValue(marketName, out var balancesData))
                {
                    return new List<ExchangeBalance>();
                }

                return balancesData.Values.ToList();
            }
        }

        public ExchangeMarketInfo GetMarketInfo(string marketName, string symbol)
        {
            lock (_sync)
            {
                if (!_markets.TryGetValue(marketName, out var marketData) ||
                    !marketData.TryGetValue(symbol, out var data))
                {
                    return null;
                }

                return data;
            }
        }

        public List<ExchangeMarketInfo> GetMarketInfo(string marketName)
        {
            lock (_sync)
            {
                if (!_markets.TryGetValue(marketName, out var marketData))
                {
                    return new List<ExchangeMarketInfo>();
                }

                return marketData.Values.ToList();
            }
        }

        public Task RefreshData()
        {
            return DoRefresh();
        }

        public void Start()
        {
            DoRefresh().GetAwaiter().GetResult();

            _timer.Start();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}