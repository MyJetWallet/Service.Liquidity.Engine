using System.Collections.Generic;
using System.Linq;

namespace Service.Liquidity.Engine.Domain.Services.ExternalMarkets
{
    public class ExternalMarketManager : IExternalMarketManager
    {
        private readonly Dictionary<string, IExternalMarket> _markets;

        public ExternalMarketManager(IExternalMarket[] markets)
        {
            _markets = markets.ToDictionary(e => e.GetName());
        }

        public IExternalMarket GetExternalMarketByName(string name)
        {
            if (_markets.TryGetValue(name, out var market))
                return market;

            return null;
        }

        public List<string> GetMarketNames()
        {
            return _markets.Keys.ToList();
        }
    }
}