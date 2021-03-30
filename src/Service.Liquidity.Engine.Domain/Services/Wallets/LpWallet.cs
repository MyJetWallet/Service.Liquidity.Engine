using System;
using System.Collections.Generic;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;

namespace Service.Liquidity.Engine.Domain.Services.Wallets
{
    public class LpWallet : ILpWallet
    {
        private readonly string _lpName;
        private readonly string _walletId;
        private readonly IWalletBalanceService _balanceService;

        public LpWallet(
            string lpName,
            string walletId,
            IWalletBalanceService balanceService)
        {
            if (string.IsNullOrEmpty(lpName)) throw new Exception($"{nameof(lpName)} cannot ne empty");
            if (string.IsNullOrEmpty(walletId)) throw new Exception($"{nameof(walletId)} cannot ne empty");
            if (balanceService == null) throw new Exception($"{nameof(balanceService)} cannot ne empty");

            _lpName = lpName;
            _walletId = walletId;
            _balanceService = balanceService;
        }

        public string GetLpName()
        {
            return _lpName;
        }

        public List<WalletBalance> GetBalances()
        {
            var resp = _balanceService
                .GetWalletBalancesAsync(new GetWalletBalancesRequest()
                {
                    WalletId = _walletId
                })
                .GetAwaiter()
                .GetResult();

            return resp.Balances;
        }
    }
}