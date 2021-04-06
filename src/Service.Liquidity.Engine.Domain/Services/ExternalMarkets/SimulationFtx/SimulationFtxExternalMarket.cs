using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Orders;
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
            var data = await _service.GetMarketInfoAsync(new GetMarketInfoRequest()
            {
                Market = market
            });

            var instrument = _instrumentDictionaryClient.GetSpotInstrumentById(new SpotInstrumentIdentity()
            {
                BrokerId = "jetwallet",
                Symbol = market.Replace("/", "")
            });

            if (instrument == null)
                return null;

            var baseAsset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = instrument.BrokerId,
                Symbol = instrument.BaseAsset
            });

            if (baseAsset == null)
                return null;

            var quoteAsset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = instrument.BrokerId,
                Symbol = instrument.QuoteAsset
            });

            if (quoteAsset == null)
                return null;

            var prm = market.Split("/");
            if (prm.Length != 2)
            {
                return null;
            }

            var resp = new ExchangeMarketInfo()
            {
                Market = market,
                MinVolume = (double)(instrument?.MinVolume ?? 0m),
                PriceAccuracy = instrument.Accuracy,
                BaseAsset = prm[0],
                QuoteAsset = prm[1],
                VolumeAccuracy = baseAsset.Accuracy,
                OppositeVolumeAccuracy = quoteAsset.Accuracy,
            };

            return resp;
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
                throw new Exception("Cannot execute trade, market info do not found. Request: {JsonConvert.SerializeObject(request)}");
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
                OppositeVolume = Math.Round(resp.Trade.Price * resp.Trade.Size, marketInfo.OppositeVolumeAccuracy,  MidpointRounding.ToZero),
                Source = GetName()
            };

            return result;
        }
    }
}