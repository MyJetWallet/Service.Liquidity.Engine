using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.NoSql;

namespace Service.Liquidity.Engine.Domain.Services.Wallets
{
    public class LpWalletManager : ILpWalletManager, IStartable
    {
        private readonly ILogger<LpWalletManager> _logger;
        private readonly IMyNoSqlServerDataWriter<LpWalletNoSql> _noSqlDataWriter;
        private readonly IWalletBalanceService _walletBalanceService;

        private readonly Dictionary<string, LpWallet> _data = new Dictionary<string, LpWallet>();
        private readonly object _sync = new object();

        public LpWalletManager(
            ILogger<LpWalletManager> logger,
            IMyNoSqlServerDataWriter<LpWalletNoSql> noSqlDataWriter,
            IWalletBalanceService walletBalanceService)
        {
            _logger = logger;
            _noSqlDataWriter = noSqlDataWriter;
            _walletBalanceService = walletBalanceService;
        }

        public List<WalletBalance> GetBalances(string walletName)
        {
            LpWallet wallet = GetWallet(walletName);

            if (wallet == null)
                return new List<WalletBalance>();

            var resp = _walletBalanceService
                .GetWalletBalancesAsync(new GetWalletBalancesRequest() {WalletId = wallet.WalletId}).GetAwaiter()
                .GetResult();

            return resp?.Balances ?? new List<WalletBalance>();
        }

        public LpWallet GetWallet(string walletName)
        {
            lock (_sync)
            {
                if (!_data.TryGetValue(walletName, out var wallet))
                    return null;

                return wallet;
            }
        }

        public LpWallet GetWalletById(string walletId)
        {
            lock (_sync)
            {
                var wallet = _data.Values.FirstOrDefault(e => e.WalletId == walletId);
                return wallet;
            }
        }

        public async Task AddWalletAsync(LpWallet wallet)
        {
            var entity = LpWalletNoSql.Create(wallet);

            await _noSqlDataWriter.InsertOrReplaceAsync(entity);

            lock (_sync)
            {
                _data[wallet.Name] = wallet;
            }

            _logger.LogInformation("Added Wallet {name}: {jsonText}", wallet.Name, JsonConvert.SerializeObject(wallet));
        }

        public async Task RemoveWalletAsync(string name)
        {
            await _noSqlDataWriter.DeleteAsync(LpWalletNoSql.GeneratePartitionKey(), LpWalletNoSql.GenerateRowKey(name));

            lock (_sync)
            {
                _data.Remove(name);
            }

            _logger.LogInformation("Deleted Wallet {name}", name);
        }

        public List<LpWallet> GetAll()
        {
            lock (_sync)
            {
                return _data.Values.ToList();
            }
        }

        public void Start()
        {
            var data = _noSqlDataWriter.GetAsync().GetAwaiter().GetResult();

            lock (_sync)
            {
                _data.Clear();
                foreach (var item in data)
                {
                    _data[item.Wallet.Name] = item.Wallet;
                }
            }
        }
    }
}