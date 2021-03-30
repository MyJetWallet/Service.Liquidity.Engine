using System.Collections.Generic;
using System.Linq;

namespace Service.Liquidity.Engine.Domain.Services.Wallets
{
    public class LpWalletManager : ILpWalletManager
    {
        private readonly Dictionary<string, ILpWallet> _map;

        public LpWalletManager(ILpWallet[] lpWallets)
        {
            _map = lpWallets.ToDictionary(e => e.GetLpName());
        }

        public ILpWallet GetWalletByLiquidityProvider(string lpName)
        {
            if (!_map.TryGetValue(lpName, out var wallet))
                return null;

            return wallet;
        }
    }
}