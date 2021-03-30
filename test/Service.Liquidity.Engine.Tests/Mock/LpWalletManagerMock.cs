using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.Services.Wallets;

namespace Service.Liquidity.Engine.Tests
{
    public class LpWalletManagerMock: ILpWalletManager
    {
        public Dictionary<string, Dictionary<string, WalletBalance>> Balances { get; set; }= new();
        public Dictionary<string, LpWallet> Wallets { get; set; } = new();

        public List<WalletBalance> GetBalances(string walletName)
        {
            if (Balances.TryGetValue(walletName, out var data))
            {
                return data.Values.ToList();
            }

            return new List<WalletBalance>();
        }

        public LpWallet GetWallet(string walletName)
        {
            if (!Wallets.TryGetValue(walletName, out var wallet))
            {
                return null;
            }

            return wallet;
        }

        public Task AddWalletAsync(LpWallet wallet)
        {
            throw new NotImplementedException();
        }

        public Task RemoveWalletAsync(string name)
        {
            throw new NotImplementedException();
        }

        public List<LpWallet> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}