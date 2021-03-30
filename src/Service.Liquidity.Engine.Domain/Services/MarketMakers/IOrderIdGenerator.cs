namespace Service.Liquidity.Engine.Domain.Services.MarketMakers
{
    public interface IOrderIdGenerator
    {
        string GenerateOrderId();
        string GenerateOrderId(string baseOrder, int index);
        string GenerateBase();
    }
}