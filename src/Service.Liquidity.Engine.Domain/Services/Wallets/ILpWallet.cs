using System.Collections.Generic;
using Service.Balances.Domain.Models;

namespace Service.Liquidity.Engine.Domain.Services.Wallets
{
    public interface ILpWallet
    {
        string GetLpName();

        List<WalletBalance> GetBalances();
    }
}