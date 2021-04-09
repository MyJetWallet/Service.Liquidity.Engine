using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
using Newtonsoft.Json;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets
{
    public class ExternalMarketManager : IExternalMarketManager, IStartable
    {
        private readonly Dictionary<string, IExternalMarket> _markets = new();

        private readonly IExternalMarket[] _sources;
        private readonly ILogger<ExternalMarketManager> _logger;
        private bool _isAllSourcesLoaded = false;

        public ExternalMarketManager(IExternalMarket[] markets, ILogger<ExternalMarketManager> logger)
        {
            _sources = markets;
            _logger = logger;
            //_markets = markets.ToDictionary(e => e.GetName());
        }

        public IExternalMarket GetExternalMarketByName(string name)
        {
            if (_markets.TryGetValue(name, out var market))
                return market;

            if (!_isAllSourcesLoaded)
            {
                Start();
                if (_markets.TryGetValue(name, out market))
                    return market;
            }

            return null;
        }

        public List<string> GetMarketNames()
        {
            return _markets.Keys.ToList();
        }

        public void Start()
        {
            _isAllSourcesLoaded = true;
            _markets.Clear();

            foreach (var source in _sources)
            {
                try
                {
                    var name = source.GetNameAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(name?.Name))
                        _markets[name.Name] = source;
                }
                catch(Exception ex)
                {
                    _isAllSourcesLoaded = false;
                    _logger.LogError(ex ,"Cannot load one of ExternalMarket");
                }
            }

            _logger.LogInformation($"Load ExternalMarket is finished: {JsonConvert.SerializeObject(_markets.Keys.ToArray())}");
        }
    }
}