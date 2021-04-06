using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.NoSql;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public class HedgeSettingsManager : IHedgeSettingsManager, IHedgeInstrumentSettingsManager
    {
        private readonly ILogger<MarketMakerSettingsManager> _logger;
        private readonly IMyNoSqlServerDataWriter<SettingsHedgeGlobalNoSql> _hedgeSettingsDataWriter;
        private readonly IMyNoSqlServerDataWriter<SettingsHedgeInstrumentNoSql> _hedgeInstrumentSettingsDataWriter;

        private HedgeSettings _globalSettings = new() {Mode = EngineMode.Disabled};
        private List<HedgeInstrumentSettings> _instrumentSettings = new();

        private readonly object _sync = new();

        public HedgeSettingsManager(
            ILogger<MarketMakerSettingsManager> logger,
            IMyNoSqlServerDataWriter<SettingsHedgeGlobalNoSql> hedgeSettingsDataWriter,
            IMyNoSqlServerDataWriter<SettingsHedgeInstrumentNoSql> hedgeInstrumentSettingsDataWriter)
        {
            _logger = logger;
            _hedgeSettingsDataWriter = hedgeSettingsDataWriter;
            _hedgeInstrumentSettingsDataWriter = hedgeInstrumentSettingsDataWriter;
        }

        public HedgeSettings GetGlobalHedgeSettings()
        {
            lock (_sync)
            {
                return _globalSettings;
            }
        }

        public async Task UpdateSettingsAsync(HedgeSettings settings)
        {
            var entity = SettingsHedgeGlobalNoSql.Create(settings);
            await _hedgeSettingsDataWriter.InsertOrReplaceAsync(entity);
            await ReloadSettingsAsync();

            _logger.LogInformation("Updated HedgeSettings: {settingsJson}", JsonConvert.SerializeObject(settings));
        }

        public List<HedgeInstrumentSettings> GetHedgeInstrumentSettingsList()
        {
            lock (_sync)
            {
                return _instrumentSettings ?? new List<HedgeInstrumentSettings>();
            }
        }

        public async Task AddOrUpdateSettings(HedgeInstrumentSettings settings)
        {
            var entity = SettingsHedgeInstrumentNoSql.Create(settings);
            await _hedgeInstrumentSettingsDataWriter.InsertOrReplaceAsync(entity);
            await ReloadSettingsAsync();

            _logger.LogInformation("Updated HedgeInstrumentSettings: {settingsJson}", JsonConvert.SerializeObject(settings));
        }

        public async Task RemoveSettings(string symbol, string walletId)
        {
            await _hedgeInstrumentSettingsDataWriter.DeleteAsync(SettingsHedgeInstrumentNoSql.GeneratePartitionKey(), SettingsHedgeInstrumentNoSql.GenerateRowKey(symbol, walletId));

            _logger.LogInformation($"Deleted HedgeInstrumentSettings. Symbol: {symbol}; Wallet: {walletId}", symbol, walletId);
        }

        public HedgeInstrumentSettings GetHedgeInstrumentSettings(string symbol, string walletId)
        {
            lock (_sync)
            {
                return _instrumentSettings.FirstOrDefault(e => e.WalletId == walletId && e.InstrumentSymbol == symbol);
            }
        }

        public void Start()
        {
            ReloadSettingsAsync().GetAwaiter().GetResult();
        }

        private async Task ReloadSettingsAsync()
        {
            var settingEntity = await _hedgeSettingsDataWriter.GetAsync(SettingsHedgeGlobalNoSql.GeneratePartitionKey(), SettingsHedgeGlobalNoSql.GenerateRowKey());

            if (settingEntity?.Settings != null)
            {
                lock (_sync)
                {
                    _globalSettings = settingEntity.Settings;
                }
            }
            else
            {
                _logger.LogWarning($"Do not found HedgeSettings in table {SettingsHedgeGlobalNoSql.TableName}");
            }

            var instrumentSettings = await _hedgeInstrumentSettingsDataWriter.GetAsync(SettingsHedgeInstrumentNoSql.GeneratePartitionKey());

            if (instrumentSettings != null)
            {
                lock (_sync)
                {
                    _instrumentSettings = instrumentSettings.Select(e => e.Settings).Where(e => e != null).ToList();
                }
            }
        }
    }
}