using System.Collections.Generic;
using System.Linq;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;

namespace Service.Liquidity.Engine.Domain.Services.OrderBooks
{
    public class OrderBookManager : IOrderBookManager
    {
        private readonly Dictionary<string, IOrderBookSource> _orderBookSources;

        public OrderBookManager(IOrderBookSource[] orderBookSources)
        {
            _orderBookSources = orderBookSources.ToDictionary(e => e.GetName());
        }

        public LeOrderBook GetOrderBook(string symbol, string source)
        {
            if (!_orderBookSources.TryGetValue(source, out var bookSource))
            {
                return null;
            }

            return bookSource.GetOrderBook(symbol);
        }

        public Dictionary<string, List<string>> GetSourcesAndSymbols()
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var source in _orderBookSources.Values)
            {
                result[source.GetName()] = source.GetSymbols();
            }

            return result;
        }

        public List<string> GetSymbols(string source)
        {
            if (!_orderBookSources.TryGetValue(source, out var bookSource))
            {
                return new List<string>();
            }

            return bookSource.GetSymbols();
        }

        public List<string> GetSourcesWithSymbol(string symbol)
        {
            var result = new List<string>();
            foreach (var source in _orderBookSources.Values.Where(e => e.HasSymbol(symbol)))
            {
                result.Add(source.GetName());
            }

            return result;
        }

        public List<string> GetSources()
        {
            return _orderBookSources.Keys.ToList();
        }
    }
}