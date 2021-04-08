using System;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;
using Service.TradeHistory.Domain.Models;

namespace Service.Liquidity.Engine.Domain.Models.Portfolio
{
    [DataContract]
    public class PositionPortfolio
    {
        public const string TopicName = "spot-liquidity-engine-position-update";

        [DataMember(Order = 1)] public string Id { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public bool IsOpen { get; set; }

        [DataMember(Order = 4)] public string Symbol { get; set; }
        [DataMember(Order = 5)] public string BaseAsset { get; set; }
        [DataMember(Order = 6)] public string QuotesAsset { get; set; }
        
        [DataMember(Order = 7)] public OrderSide Side { get; set; }

        [DataMember(Order = 8)] public decimal BaseVolume { get; set; }
        [DataMember(Order = 9)] public decimal QuoteVolume { get; set; }

        [DataMember(Order = 10)] public DateTime OpenTime { get; set; }
        [DataMember(Order = 11)] public DateTime? CloseTime { get; set; }

        [DataMember(Order = 12)] public decimal QuoteAssetToUsdPrice { get; set; }
        [DataMember(Order = 13)] public decimal PLUsd { get; set; }

        [DataMember(Order = 14)] public decimal TotalBaseVolume { get; set; }
        [DataMember(Order = 15)] public decimal TotalQuoteVolume { get; set; }
        [DataMember(Order = 15)] public decimal ResultPercentage { get; set; }

        public decimal ApplyTrade(OrderSide side, decimal price, decimal volume)
        {
            if ((side == OrderSide.Buy && volume < 0) || (side == OrderSide.Sell && volume > 0))
                throw new Exception($"Bad parameters, trade is {side}, but volume is {volume}. PositionId: {Id}");

            if (price <= 0)
                throw new Exception($"Bad parameters, wrong price: {price}. PositionId: {Id}");

            if (Side == side)
            {
                BaseVolume += volume;
                QuoteVolume += -1 * price * volume;
                CloseTime = DateTime.UtcNow;

                TotalBaseVolume += volume;
                TotalQuoteVolume += -1 * price * volume;

                return 0m;
            }

            var applyVolume = Math.Abs(volume) <= Math.Abs(BaseVolume) ? volume : -BaseVolume;

            var quoteTradeVolume = -1 * price * applyVolume;

            BaseVolume += applyVolume;
            QuoteVolume += quoteTradeVolume;

            if (BaseVolume == 0m)
            {
                IsOpen = false;
                QuoteVolume = Math.Round(QuoteVolume, 6);
                TotalBaseVolume = Math.Round(TotalBaseVolume, 6);
                TotalQuoteVolume = Math.Round(TotalQuoteVolume, 6);
                ResultPercentage = Math.Round(QuoteVolume / TotalQuoteVolume * 100, 2);
            }

            CloseTime = DateTime.UtcNow;

            return volume - applyVolume;
        }

        public void ApplyClosePl(decimal quoteAssetToUsdPrice)
        {
            if (IsOpen)
                throw new Exception($"Cannot calculate PL by position {Id}, position still open");

            if (quoteAssetToUsdPrice <= 0)
                throw new Exception($"Bad parameters, wrong price: {quoteAssetToUsdPrice}. PositionId: {Id}");

            QuoteAssetToUsdPrice = quoteAssetToUsdPrice;

            PLUsd = Math.Round(QuoteVolume * QuoteAssetToUsdPrice);
        }
    }
}