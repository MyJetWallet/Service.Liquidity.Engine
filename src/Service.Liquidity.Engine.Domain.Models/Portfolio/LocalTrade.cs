using System;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;
using Service.TradeHistory.Domain.Models;

namespace Service.Liquidity.Engine.Domain.Models.Portfolio
{
    [DataContract]
    public class PortfolioTrade
    {
        [DataMember(Order = 1)] public string TradeId { get; set; }
        [DataMember(Order = 2)] public string Source { get; set; }
        [DataMember(Order = 2)] public bool IsInternal { get; set; }
        [DataMember(Order = 3)] public string Symbol { get; set; }
        [DataMember(Order = 4)] public OrderSide Side { get; set; }
        [DataMember(Order = 5)] public double Price { get; set; }
        [DataMember(Order = 6)] public double BaseVolume { get; set; }
        [DataMember(Order = 7)] public double QuoteVolume { get; set; }
        [DataMember(Order = 8)] public DateTime DateTime { get; set; }
        [DataMember(Order = 9)] public string ReferenceId { get; set; }

        public PortfolioTrade(WalletTrade trade, string walletName)
        {
            Source = walletName;
            IsInternal = true;
            Symbol = trade.InstrumentSymbol;
            Price = trade.Price;
            BaseVolume = trade.BaseVolume;
            QuoteVolume = trade.QuoteVolume;
            DateTime = trade.DateTime;
            TradeId = trade.TradeUId;
            Side = trade.Side;
            ReferenceId = string.Empty;
        }

        public PortfolioTrade(string tradeId, string source, bool isInternal, string symbol, OrderSide side, double price, double baseVolume, 
            double quoteVolume, DateTime dateTime, string referenceId)
        {
            TradeId = tradeId;
            Source = source;
            IsInternal = isInternal;
            Symbol = symbol;
            Side = side;
            Price = price;
            BaseVolume = baseVolume;
            QuoteVolume = quoteVolume;
            DateTime = dateTime;
            ReferenceId = referenceId;
        }
    }

    [DataContract]
    public class PositionAssociation
    {
        [DataMember(Order = 1)] public string PositionId { get; set; }
        [DataMember(Order = 2)] public string TradeId { get; set; }
        [DataMember(Order = 3)] public string Source { get; set; }
        [DataMember(Order = 4)] public bool IsInternalTrade { get; set; }

        public PositionAssociation()
        {
        }

        public PositionAssociation(string positionId, string tradeId, string source)
        {
            PositionId = positionId;
            TradeId = tradeId;
            Source = source;
        }
    }



}