using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.AssetsDictionary.Client;
using Service.Liquidity.Engine.Domain.Models.ExternalMarkets;
using Service.Simulation.FTX.Grpc;
using Service.Simulation.FTX.Grpc.Models;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets.SimulationFtx
{
    public class SimulationFtxExternalMarket: IExternalMarket
    {
        private readonly ISimulationFtxTradingService _service;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionaryClient;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;

        private Dictionary<string, ExchangeMarketInfo> _marketInfoData = new Dictionary<string, ExchangeMarketInfo>();

        public SimulationFtxExternalMarket(ISimulationFtxTradingService service, ISpotInstrumentDictionaryClient instrumentDictionaryClient, IAssetsDictionaryClient assetsDictionaryClient)
        {
            _service = service;
            _instrumentDictionaryClient = instrumentDictionaryClient;
            _assetsDictionaryClient = assetsDictionaryClient;
        }

        public string GetName()
        {
            return "Simulation-FTX";
        }

        public async Task<double> GetBalance(string asset)
        {
            var resp = await _service.GetBalancesAsync();
            var balance = resp.Balances.FirstOrDefault(e => e.Symbol == asset)?.Amount ?? 0;
            return balance;
        }

        public async Task<Dictionary<string, double>> GetBalances()
        {
            var resp = await _service.GetBalancesAsync();
            var result = resp.Balances.ToDictionary(e => e.Symbol, e => e.Amount);
            return result;
        }

        public async Task<ExchangeMarketInfo> GetMarketInfo(string market)
        {
            if(_marketInfoData == null)
                await LoadMarketInfo();

            if (_marketInfoData.TryGetValue(market, out var resp))
                return resp;

            return null;
        }

        public async Task<List<ExchangeMarketInfo>> GetMarketInfoListAsync()
        {
            if (_marketInfoData == null)
                await LoadMarketInfo();

            return _marketInfoData.Values.ToList();
        }

        private async Task LoadMarketInfo()
        {
            using var activity = MyTelemetry.StartActivity("Load market info");
            try
            {
                var data = await _service.GetMarketInfoListAsync();

                var result = new Dictionary<string, ExchangeMarketInfo>();

                foreach (var marketInfo in data.Info)
                {
                    var resp = new ExchangeMarketInfo()
                    {
                        Market = marketInfo.Market,
                        MinVolume = marketInfo.MinVolume,
                        PriceAccuracy = marketInfo.PriceAccuracy,
                        BaseAsset = marketInfo.BaseAsset,
                        QuoteAsset = marketInfo.QuoteAsset,
                        VolumeAccuracy = marketInfo.BaseAccuracy
                    };

                    result[resp.Market] = resp;
                }

                _marketInfoData = result;
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                throw;
            }
        }

        public async Task<ExchangeTrade> MarketTrade(string market, OrderSide side, double volume, string referenceId)
        {
            var request = new ExecuteMarketOrderRequest()
            {
                ClientId = referenceId,
                Market = market,
                Side = side == OrderSide.Buy ? SimulationFtxOrderSide.Buy : SimulationFtxOrderSide.Sell,
                Size = volume
            };

            

            var marketInfo = await GetMarketInfo(market);
            if (marketInfo == null)
            {
                throw new Exception($"Cannot execute trade, market info do not found. Request: {JsonConvert.SerializeObject(request)}");
            }

            var resp = await _service.ExecuteMarketOrderAsync(request);

            if (!resp.Success)
            {
                throw new Exception("Cannot execute trade in simulation ftx. Request: {JsonConvert.SerializeObject(request)}");
            }

            var result = new ExchangeTrade()
            {
                Id = resp.Trade.Id,
                Market = resp.Trade.Market,
                Price = resp.Trade.Price,
                Volume = resp.Trade.Size,
                Timestamp = resp.Trade.Timestamp,
                Side = resp.Trade.Side == SimulationFtxOrderSide.Buy ? OrderSide.Buy : OrderSide.Sell,
                OppositeVolume = (double)((decimal)resp.Trade.Price * (decimal)resp.Trade.Size),
                Source = GetName()
            };

            return result;
        }
    }
}