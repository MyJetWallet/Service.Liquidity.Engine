using System.Threading.Tasks;

namespace Service.Liquidity.Engine.Domain.Services.MarketMakers
{
    public interface IMarketMaker
    {
        Task RefreshOrders();
    }
}