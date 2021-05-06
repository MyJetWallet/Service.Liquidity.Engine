using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.NoSql;
using Service.Liquidity.Engine.Domain.Services.Wallets;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public class MarketMakerSettingsManager : IMarketMakerSettingsManager, IMarketMakerSettingsAccessor
    {
        private readonly ILogger<MarketMakerSettingsManager> _logger;
        private readonly IMyNoSqlServerDataWriter<SettingsMarketMakerNoSql> _marketMakerDataWriter;
        private readonly IMyNoSqlServerDataWriter<SettingsLiquidityProviderInstrumentNoSql> _lpInstrumentDataWriter;

        private readonly ILpWalletManager _lpWalletManager;

        private Dictionary<string, LiquidityProviderInstrumentSettings> _lpInstrumentSettings = new();
        private MarketMakerSettings _marketMakerSettings = new MarketMakerSettings(EngineMode.Disabled);

        private readonly object _sync = new object();

        public MarketMakerSettingsManager(
            ILogger<MarketMakerSettingsManager> logger,
            IMyNoSqlServerDataWriter<SettingsMarketMakerNoSql> marketMakerDataWriter,
            IMyNoSqlServerDataWriter<SettingsLiquidityProviderInstrumentNoSql> lpInstrumentDataWriter,
            ILpWalletManager lpWalletManager)
        {
            _logger = logger;
            _marketMakerDataWriter = marketMakerDataWriter;
            _lpInstrumentDataWriter = lpInstrumentDataWriter;
            _lpWalletManager = lpWalletManager;
        }

        public async Task ChangeMarketMakerModeAsync(EngineMode mode)
        {
            MarketMakerSettings settings;
            lock (_sync)
            {
                _marketMakerSettings.Mode = mode;
                settings = _marketMakerSettings;
            }

            await _marketMakerDataWriter.InsertOrReplaceAsync(SettingsMarketMakerNoSql.Create(settings));

            _logger.LogInformation("Market Maker mode was changed to {modeText}", mode.ToString());
        }

        public async Task UpdateMarketMakerSettingsAsync(MarketMakerSettings settings)
        {
            using var action = MyTelemetry.StartActivity("Update MarketMaker Settings");
            settings.AddToActivityAsJsonTag("settings");

            try
            {
                var entity = SettingsMarketMakerNoSql.Create(settings);
                await _marketMakerDataWriter.InsertOrReplaceAsync(entity);
                await ReloadSettings();

                _logger.LogInformation("Updated market maker settings: {jsonText}",
                    JsonConvert.SerializeObject(settings));
            }
            catch(Exception ex)
            {
                ex.FailActivity();
                throw;
            }
        }

        public MarketMakerSettings GetMarketMakerSettings()
        {
            lock (_sync)
            {
                return _marketMakerSettings;
            }
        }

        public List<LiquidityProviderInstrumentSettings> GetLiquidityProviderSettings()
        {
            lock (_sync)
            {
                return _lpInstrumentSettings.Values.ToList();
            }
        }

        public async Task AddLiquidityProviderSettings(LiquidityProviderInstrumentSettings settings)
        {
            using var action = MyTelemetry.StartActivity("Add Liquidity Provider Instrument");
            settings.AddToActivityAsJsonTag("settings");

            try
            {
                settings.LpHedges ??= new();
                settings.LpSources ??= new();

                FillWallet(settings);

                ValidateSettings(settings);

                var entity = SettingsLiquidityProviderInstrumentNoSql.Create(settings);

                var exist = await _lpInstrumentDataWriter.GetAsync(entity.PartitionKey, entity.RowKey);

                if (exist != null)
                {
                    _logger.LogError(
                        "Cannot add new Liquidity Provider Instrument, because already exist provider for symbol. Request: {jsonText}",
                        JsonConvert.SerializeObject(settings));
                    throw new Exception(
                        $"Cannot add new Mirroring Liquidity Provider, because already exist provider for symbol {settings.Symbol}");
                }

                await _lpInstrumentDataWriter.InsertOrReplaceAsync(entity);

                await ReloadSettings();

                _logger.LogInformation("Added Liquidity Provider Instrument: {jsonText}",
                    JsonConvert.SerializeObject(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add AddLiquidityProviderSettings: {requestJson}", JsonConvert.SerializeObject(settings));
                ex.FailActivity();
                throw;
            }
        }

        

        public async Task UpdateLiquidityProviderSettings(LiquidityProviderInstrumentSettings settings)
        {
            using var action = MyTelemetry.StartActivity("Update Liquidity Provider Instrument");
            settings.AddToActivityAsJsonTag("settings");

            try
            {
                FillWallet(settings);

                ValidateSettings(settings);

                await _lpInstrumentDataWriter.InsertOrReplaceAsync(SettingsLiquidityProviderInstrumentNoSql.Create(settings));

                await ReloadSettings();

                _logger.LogInformation("Updated Liquidity Provider Instrument: {jsonText}", JsonConvert.SerializeObject(settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add UpdateLiquidityProviderSettings: {requestJson}", JsonConvert.SerializeObject(settings));
                ex.FailActivity();
                throw;
            }
        }

        public async Task RemoveLiquidityProviderSettings(string symbol)
        {
            using var action = MyTelemetry.StartActivity("Update Liquidity Provider Instrument");
            new { symbol }.AddToActivityAsJsonTag("settings");

            try
            {
                var entity = await _lpInstrumentDataWriter.DeleteAsync(SettingsLiquidityProviderInstrumentNoSql.GeneratePartitionKey(), SettingsLiquidityProviderInstrumentNoSql.GenerateRowKey(symbol));

                if (entity != null)
                    _logger.LogInformation("Removed Liquidity Provider Instrument: {jsonText}", JsonConvert.SerializeObject(entity.Settings));

                await ReloadSettings();

                _logger.LogInformation("Removed Liquidity Provider Instrument: {symbol}", symbol);
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                throw;
            }
        }

        public void Start()
        {
            ReloadSettings().GetAwaiter().GetResult();
        }

        private async Task ReloadSettings()
        {
            var marketMaker = (await _marketMakerDataWriter.GetAsync(SettingsMarketMakerNoSql.GeneratePartitionKey()))
                .FirstOrDefault();

            var lpInstrument = (await _lpInstrumentDataWriter.GetAsync(SettingsLiquidityProviderInstrumentNoSql.GeneratePartitionKey()))
                .ToList();

            lock (_sync)
            {
                if (marketMaker != null)
                    _marketMakerSettings = marketMaker.Settings;

                _lpInstrumentSettings = lpInstrument.ToDictionary(e => e.Settings.Symbol, e => e.Settings);
            }
        }

        private void FillWallet(LiquidityProviderInstrumentSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.LpWalletName))
            {
                var  wallet = _lpWalletManager.GetWallet(settings.LpWalletName);
                if (wallet == null)
                {
                    _logger.LogError($"Cannot find wallet by name: {settings.LpWalletName}");
                    throw new Exception($"Cannot find wallet by name: {settings.LpWalletName}");
                }

                settings.WalletId = wallet.WalletId;
                settings.BrokerId = wallet.BrokerId;
                return;
            }

            if (!string.IsNullOrEmpty(settings.WalletId))
            {
                var wallet = _lpWalletManager.GetWalletById(settings.WalletId);
                if (wallet == null)
                {
                    _logger.LogError($"Cannot find wallet by Id: {settings.WalletId}");
                    throw new Exception($"Cannot find wallet by Id: {settings.WalletId}");
                }

                settings.WalletId = wallet.WalletId;
                settings.BrokerId = wallet.BrokerId;
                return;
            }

            _logger.LogError("Cannot find wallet, name and Id is empty");
            throw new Exception("Cannot find wallet, name and Id is empty");
        }

        private void ValidateSettings(LiquidityProviderInstrumentSettings settings)
        {
            if (settings.LpHedges.GroupBy(e => new { e.ExternalMarket, e.ExternalSymbol }).Any(e => e.Count() > 1))
            {
                throw new Exception("Cannot add settings with duplicate in LpHedges");
            }

            if (settings.LpSources.GroupBy(e => new { e.ExternalMarket, e.ExternalSymbol }).Any(e => e.Count() > 1))
            {
                throw new Exception("Cannot add settings with duplicate in LpSources");
            }
        }

    }
}