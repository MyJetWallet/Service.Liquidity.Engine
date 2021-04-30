using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public class MarketMakerSettingsManager : IMarketMakerSettingsManager, IMarketMakerSettingsAccessor
    {
        private readonly ILogger<MarketMakerSettingsManager> _logger;
        private readonly IMyNoSqlServerDataWriter<SettingsMarketMakerNoSql> _marketMakerDataWriter;
        private readonly IMyNoSqlServerDataWriter<SettingsMirroringLiquidityNoSql> _mirrorLiquidityDataWriter;
        private readonly IMyNoSqlServerDataWriter<SettingsLiquidityProviderInstrumentNoSql> _lpInstrumentDataWriter;

        private Dictionary<string, MirroringLiquiditySettings> _mirroringLiquiditySettings = new();
        private Dictionary<string, LiquidityProviderInstrumentSettings> _lpInstrumentSettings = new();
        private MarketMakerSettings _marketMakerSettings = new MarketMakerSettings(EngineMode.Disabled);

        private readonly object _sync = new object();

        public MarketMakerSettingsManager(
            ILogger<MarketMakerSettingsManager> logger,
            IMyNoSqlServerDataWriter<SettingsMarketMakerNoSql> marketMakerDataWriter,
            IMyNoSqlServerDataWriter<SettingsMirroringLiquidityNoSql> mirrorLiquidityDataWriter,
            IMyNoSqlServerDataWriter<SettingsLiquidityProviderInstrumentNoSql> lpInstrumentDataWriter
            )
        {
            _logger = logger;
            _marketMakerDataWriter = marketMakerDataWriter;
            _mirrorLiquidityDataWriter = mirrorLiquidityDataWriter;
            _lpInstrumentDataWriter = lpInstrumentDataWriter;
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

        public async Task AddMirroringLiquiditySettingsAsync(MirroringLiquiditySettings setting)
        {
            using var action = MyTelemetry.StartActivity("Add Mirroring Liquidity Provider");
            setting.AddToActivityAsJsonTag("settings");

            try
            {
                var entity = SettingsMirroringLiquidityNoSql.Create(setting);

                var exist = await _mirrorLiquidityDataWriter.GetAsync(entity.PartitionKey, entity.RowKey);

                if (exist != null)
                {
                    _logger.LogError(
                        "Cannot add new Mirroring Liquidity Provider, because already exist provider for symbol\\wallet. Request: {jsonText}",
                        JsonConvert.SerializeObject(setting));
                    throw new Exception(
                        "Cannot add new Mirroring Liquidity Provider, because already exist provider for symbol\\wallet");
                }

                await _mirrorLiquidityDataWriter.InsertOrReplaceAsync(entity);

                await ReloadSettings();

                _logger.LogInformation("Added MirroringLiquidity Settings: {jsonText}",
                    JsonConvert.SerializeObject(setting));
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                throw;
            }
        }


        public async Task UpdateMirroringLiquiditySettingsAsync(MirroringLiquiditySettings setting)
        {
            using var action = MyTelemetry.StartActivity("Update Mirroring Liquidity Provider");
            setting.AddToActivityAsJsonTag("settings");

            try
            {
                await _mirrorLiquidityDataWriter.InsertOrReplaceAsync(SettingsMirroringLiquidityNoSql.Create(setting));

                await ReloadSettings();

                _logger.LogInformation("Updated MirroringLiquidity Settings: {jsonText}",
                    JsonConvert.SerializeObject(setting));
            }
            catch(Exception ex)
            {
                ex.FailActivity();
                throw;
            }
        }

        public async Task RemoveMirroringLiquiditySettingsAsync(string symbol, string walletName)
        {
            using var action = MyTelemetry.StartActivity("Update Mirroring Liquidity Provider");
            new {symbol, walletName}.AddToActivityAsJsonTag("settings");

            try
            {
                var entity = await _mirrorLiquidityDataWriter.DeleteAsync(SettingsMirroringLiquidityNoSql.GeneratePartitionKey(),
                    SettingsMirroringLiquidityNoSql.GenerateRowKey(symbol, walletName));

                if (entity != null)
                    _logger.LogInformation("Removed MirroringLiquidity: {jsonText}", JsonConvert.SerializeObject(entity.Settings));

                await ReloadSettings();

                _logger.LogInformation("Removed MirroringLiquidity Settings: {symbol}, {walletName}", symbol,
                    walletName);
            }
            catch(Exception ex)
            {
                ex.FailActivity();
                throw;
            }
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

        public List<MirroringLiquiditySettings> GetMirroringLiquiditySettingsList()
        {
            lock (_sync)
            {
                return _mirroringLiquiditySettings.Values.ToList();
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
                await _lpInstrumentDataWriter.InsertOrReplaceAsync(SettingsLiquidityProviderInstrumentNoSql.Create(settings));

                await ReloadSettings();

                _logger.LogInformation("Updated Liquidity Provider Instrument: {jsonText}", JsonConvert.SerializeObject(settings));
            }
            catch (Exception ex)
            {
                ex.FailActivity();

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

            var mirrorLiquidity = (await _mirrorLiquidityDataWriter.GetAsync(SettingsMirroringLiquidityNoSql.GeneratePartitionKey()))
                .ToList();

            var lpInstrument = (await _lpInstrumentDataWriter.GetAsync(SettingsLiquidityProviderInstrumentNoSql.GeneratePartitionKey()))
                .ToList();

            lock (_sync)
            {
                if (marketMaker != null)
                    _marketMakerSettings = marketMaker.Settings;

                _mirroringLiquiditySettings = mirrorLiquidity.ToDictionary(e => e.RowKey, e => e.Settings);

                _lpInstrumentSettings = lpInstrument.ToDictionary(e => e.Settings.Symbol, e => e.Settings);
            }
        }
    }
}