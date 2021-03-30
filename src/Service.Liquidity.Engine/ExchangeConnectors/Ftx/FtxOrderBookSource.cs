using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MyJetWallet.Connector.Ftx.WebSocket;
using Service.Liquidity.Engine.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;

namespace Service.Liquidity.Engine.ExchangeConnectors.Ftx
{
    public class FtxOrderBookSource: IOrderBookSource, IDisposable
    {
        private readonly Dictionary<string, string> _symbolToOriginalSymbol;

        private readonly FtxWsOrderBooks _wsFtx;

        public FtxOrderBookSource(ILoggerFactory loggerFactory, List<(string, string)> symbolAndOriginalSymbolList)
        {
            _symbolToOriginalSymbol = symbolAndOriginalSymbolList.ToDictionary(e => e.Item1, e => e.Item2);
            _wsFtx = new FtxWsOrderBooks(loggerFactory.CreateLogger<FtxWsOrderBooks>(), _symbolToOriginalSymbol.Values.ToArray());
        }

        public string GetName()
        {
            return ExchangeNames.FTX;
        }

        public List<string> GetSymbols()
        {
            return _symbolToOriginalSymbol.Keys.ToList();
        }

        public bool HasSymbol(string symbol)
        {
            return _symbolToOriginalSymbol.Keys.Contains(symbol);
        }

        public LeOrderBook GetOrderBook(string symbol)
        {
            if (!_symbolToOriginalSymbol.TryGetValue(symbol, out var originalSymbol))
            {
                return null;
            }

            var data = _wsFtx.GetOrderBookById(originalSymbol);
            
            if (data == null)
                return null;

            var book = new LeOrderBook()
            {
                Symbol = symbol,
                Timestamp = data.GetTime().UtcDateTime,
                Asks = data.asks.Select(LeOrderBookLevel.Create).Where(e => e != null).ToList(),
                Bids = data.bids.Select(LeOrderBookLevel.Create).Where(e => e != null).ToList(),
                Source = ExchangeNames.FTX,
                OriginalSymbol = originalSymbol
            };

            return book;
        }

        public void Start()
        {
            _wsFtx.Start();
        }

        public void Stop()
        {
            _wsFtx.Stop();
        }


        public void Dispose()
        {
            _wsFtx?.Dispose();
        }
    }
}