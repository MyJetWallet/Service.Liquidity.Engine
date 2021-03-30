using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Wallets;

namespace Service.Liquidity.Engine.Domain.Services.Wallets
{
    public interface ILpWalletManager
    {
        List<WalletBalance> GetBalances(string walletName);

        Task AddWalletAsync(LpWallet wallet);

        Task RemoveWalletAsync(string name);

        List<LpWallet> GetAll();
    }
}