using System.Collections.Generic;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets
{
    public interface IExternalMarketManager
    {
        IExternalMarket GetExternalMarketByName(string name);

        List<string> GetMarketNames();
    }
}