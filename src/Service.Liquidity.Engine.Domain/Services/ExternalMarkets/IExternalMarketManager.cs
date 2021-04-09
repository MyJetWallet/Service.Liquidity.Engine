using System.Collections.Generic;
using MyJetWallet.Domain.ExternalMarketApi;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets
{
    public interface IExternalMarketManager
    {
        IExternalMarket GetExternalMarketByName(string name);

        List<string> GetMarketNames();
    }
}