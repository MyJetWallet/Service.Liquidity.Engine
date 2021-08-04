using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain.ExternalMarketApi;
using MyJetWallet.Domain.ExternalMarketApi.Dto;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Newtonsoft.Json;

namespace Service.Liquidity.Engine.Domain.Services.OrderBooks
{
    public class OrderBookManager : IOrderBookManager, IStartable
    {
        private readonly Dictionary<string, IOrderBookSource> _orderBookSources = new();

        private IOrderBookSource[] _sources;
        private readonly ILogger<OrderBookManager> _logger;
        private bool _isAllLoaded = false;

        public OrderBookManager(IOrderBookSource[] orderBookSources, ILogger<OrderBookManager> logger)
        {
            _sources = orderBookSources;
            _logger = logger;
            //_orderBookSources = orderBookSources.ToDictionary(e => e.GetName());
        }

        public void Start()
        {
            _isAllLoaded = true;
            _orderBookSources.Clear();

            foreach (var source in _sources)
            {
                try
                {
                    var name = source.GetNameAsync(null).GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(name?.Name))
                    {
                        _orderBookSources[name.Name] = source;
                    }
                }
                catch (Exception ex)
                {
                    _isAllLoaded = false;
                    _logger.LogError(ex, "Cannot load one of IOrderBookSource");
                }
            }

            _logger.LogInformation($"Load IOrderBookSource is finished: {JsonConvert.SerializeObject(_orderBookSources.Keys.ToArray())}");
        }

        public async Task<LeOrderBook> GetOrderBook(string symbol, string source)
        {
            if (!_orderBookSources.TryGetValue(source, out var bookSource))
            {
                if (!_isAllLoaded)
                {
                    Start();
                    if (!_orderBookSources.TryGetValue(source, out bookSource))
                        return null;
                }

                return null;
            }

            var resp = await bookSource.GetOrderBookAsync(new MarketRequest(){Market = symbol });

            return resp?.OrderBook;
        }

        public async Task<Dictionary<string, List<string>>> GetSourcesAndSymbols()
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var source in _orderBookSources)
            {
                var data = await source.Value.GetSymbolsAsync(null);
                if (data?.Symbols != null)
                {
                    result[source.Key] = data.Symbols;
                }
            }

            return result;
        }

        public async Task<List<string>> GetSymbols(string source)
        {
            if (!_orderBookSources.TryGetValue(source, out var bookSource))
            {
                return new List<string>();
            }

            var resp =  await bookSource.GetSymbolsAsync(null);

            return resp?.Symbols ?? new List<string>();
        }

        public async Task<List<string>> GetSourcesWithSymbol(string symbol)
        {
            var result = new List<string>();

            var data = await GetSourcesAndSymbols();

            foreach (var source in data.Where(e => e.Value.Contains(symbol)))
            {
                result.Add(source.Key);
            }

            return result;
        }

        public Task<List<string>> GetSources()
        {
            return Task.FromResult(_orderBookSources.Keys.ToList());
        }
    }
}