using System.Runtime.Serialization;

namespace Service.Liquidity.Engine.Domain.Models.Portfolio
{
    [DataContract]
    public class PositionAssociation
    {
        public const string TopicName = "spot-liquidity-engine-position-association";

        [DataMember(Order = 1)] public string PositionId { get; set; }
        [DataMember(Order = 2)] public string TradeId { get; set; }
        [DataMember(Order = 3)] public string Source { get; set; }
        [DataMember(Order = 4)] public bool IsInternalTrade { get; set; }

        public PositionAssociation()
        {
        }

        public PositionAssociation(string positionId, string tradeId, string source, bool isInternal)
        {
            PositionId = positionId;
            TradeId = tradeId;
            Source = source;
            IsInternalTrade = isInternal;
        }
    }
}