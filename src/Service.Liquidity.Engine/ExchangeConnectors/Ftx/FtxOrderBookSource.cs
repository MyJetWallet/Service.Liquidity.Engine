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
        private readonly List<string> _symbolList;

        private readonly FtxWsOrderBooks _wsFtx;

        public FtxOrderBookSource(ILoggerFactory loggerFactory, List<string> symbolList)
        {
            _symbolList = symbolList;
            _wsFtx = new FtxWsOrderBooks(loggerFactory.CreateLogger<FtxWsOrderBooks>(), _symbolList.ToArray());
        }

        public string GetName()
        {
            return ExchangeNames.FTX;
        }

        public List<string> GetSymbols()
        {
            return _symbolList.ToList();
        }

        public bool HasSymbol(string symbol)
        {
            return _symbolList.Contains(symbol);
        }

        public LeOrderBook GetOrderBook(string symbol)
        {
            if (!_symbolList.Contains(symbol))
            {
                return null;
            }

            var data = _wsFtx.GetOrderBookById(symbol);
            
            if (data == null)
                return null;

            var book = new LeOrderBook()
            {
                Symbol = symbol,
                Timestamp = data.GetTime().UtcDateTime,
                Asks = data.asks.Select(LeOrderBookLevel.Create).Where(e => e != null).ToList(),
                Bids = data.bids.Select(LeOrderBookLevel.Create).Where(e => e != null).ToList(),
                Source = ExchangeNames.FTX
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