using System;
using System.Collections.Generic;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;

namespace Service.Liquidity.Engine.Tests
{
    public class OrderBookManagerMock: IOrderBookManager
    {
        public Dictionary<(string, string), LeOrderBook> Data { get; set; } = new();

        public LeOrderBook GetOrderBook(string symbol, string source)
        {
            Data.TryGetValue((symbol, source), out var book);
            return book;
        }

        public Dictionary<string, List<string>> GetSourcesAndSymbols()
        {
            throw new NotImplementedException();
        }

        public List<string> GetSymbols(string source)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSourcesWithSymbol(string symbol)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSources()
        {
            throw new NotImplementedException();
        }
    }
}