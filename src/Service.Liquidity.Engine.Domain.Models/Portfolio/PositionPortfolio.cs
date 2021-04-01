using System;
using System.Runtime.Serialization;
using Service.TradeHistory.Domain.Models;

namespace Service.Liquidity.Engine.Domain.Models.Portfolio
{
    [DataContract]
    public class PositionPortfolio
    {
        [DataMember(Order = 1)]
        public string Id
        {
            get { return OpenTrade?.TradeUId; }
            set { }
        }

        [DataMember(Order = 2)] public WalletTrade OpenTrade { get; set; }

        [DataMember(Order = 3)] public HedgeTradeInfo CloseTrade { get; set; }

        [DataMember(Order = 4)] public double PL { get; set; }

        [DataMember(Order = 5)] public double QuoteCurrencyToUsdPrice { get; set; }
        [DataMember(Order = 6)] public double PLUsd { get; set; }



        [DataMember(Order = 7)] public TimeSpan Delay { get; set; }

        public void ApplyCloseTrade(HedgeTradeInfo trade, decimal quoteCurrencyToUsdPrice)
        {
            CloseTrade = trade;

            var diff = ((decimal) CloseTrade.Price - (decimal) OpenTrade.Price) * (decimal) OpenTrade.BaseVolume;

            PL = (double) diff;

            Delay = (CloseTrade.Timestamp - OpenTrade.DateTime);

            QuoteCurrencyToUsdPrice = (double) quoteCurrencyToUsdPrice;
            PLUsd = (double) (diff * quoteCurrencyToUsdPrice);
        }
    }
}