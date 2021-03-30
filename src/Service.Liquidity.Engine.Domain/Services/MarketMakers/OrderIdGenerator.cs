using System;

namespace Service.Liquidity.Engine.Domain.Services.MarketMakers
{
    public class OrderIdGenerator : IOrderIdGenerator
    {
        public string GenerateOrderId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string GenerateOrderId(string baseOrder, int index)
        {
            return $"{baseOrder}-{index}";
        }

        public string GenerateBase()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}