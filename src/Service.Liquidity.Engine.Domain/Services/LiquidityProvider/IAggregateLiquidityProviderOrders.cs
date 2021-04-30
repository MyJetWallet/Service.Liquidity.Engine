using System.Collections.Generic;
using Service.Liquidity.Engine.Domain.Models.LiquidityProvider;

namespace Service.Liquidity.Engine.Domain.Services.LiquidityProvider
{
    public interface IAggregateLiquidityProviderOrders
    {
        List<LpOrder> GetCurrentOrders();
    }
}