using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain.ExternalMarketApi.Models;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class OrderBookManagerMock: IOrderBookManager
    {
        public Dictionary<(string, string), LeOrderBook> Data { get; set; } = new();

        public Task<LeOrderBook> GetOrderBook(string symbol, string source)
        {
            Data.TryGetValue((symbol, source), out var book);
            return Task.FromResult(book);
        }

        public Task<Dictionary<string, List<string>>> GetSourcesAndSymbols()
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetSymbols(string source)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetSourcesWithSymbol(string symbol)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetSources()
        {
            throw new NotImplementedException();
        }
    }
}