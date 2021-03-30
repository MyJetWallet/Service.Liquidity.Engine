using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.NoSql;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public class MarketMakerSettingsManager : IMarketMakerSettingsManager, IMarketMakerSettingsAccessor, IStartable
    {
        private readonly ILogger<MarketMakerSettingsManager> _logger;
        private readonly IMyNoSqlServerDataWriter<SettingsMarketMakerNoSql> _marketMakerDataWriter;
        private readonly IMyNoSqlServerDataWriter<SettingsMirroringLiquidityNoSql> _mirrorLiquidityDataWriter;

        private Dictionary<string, MirroringLiquiditySettings> _mirroringLiquiditySettings = new Dictionary<string, MirroringLiquiditySettings>();
        private MarketMakerSettings _marketMakerSettings = new MarketMakerSettings(EngineMode.Disabled);

        private readonly object _sync = new object();

        public MarketMakerSettingsManager(
            ILogger<MarketMakerSettingsManager> logger,
            IMyNoSqlServerDataWriter<SettingsMarketMakerNoSql> marketMakerDataWriter,
            IMyNoSqlServerDataWriter<SettingsMirroringLiquidityNoSql> mirrorLiquidityDataWriter)
        {
            _logger = logger;
            _marketMakerDataWriter = marketMakerDataWriter;
            _mirrorLiquidityDataWriter = mirrorLiquidityDataWriter;
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

        public async Task AddOrUpdateMirroringLiquiditySettingsAsync(MirroringLiquiditySettings setting)
        {
            await _mirrorLiquidityDataWriter.InsertOrReplaceAsync(SettingsMirroringLiquidityNoSql.Create(setting));

            await ReloadSettings();

            _logger.LogInformation("Updated MirroringLiquidity Settings: {jsonText}", JsonConvert.SerializeObject(setting));
        }

        public async Task RemoveMirroringLiquiditySettingsAsync(string symbol, string walletName)
        {
            await _mirrorLiquidityDataWriter.DeleteAsync(SettingsMirroringLiquidityNoSql.GeneratePartitionKey(), SettingsMirroringLiquidityNoSql.GenerateRowKey(symbol, walletName));

            await ReloadSettings();
            
            _logger.LogInformation("Removed MirroringLiquidity Settings: {symbol}, {walletName}", symbol, walletName);
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

            lock (_sync)
            {
                if (marketMaker != null)
                    _marketMakerSettings = marketMaker.Settings;

                _mirroringLiquiditySettings = mirrorLiquidity.ToDictionary(e => e.RowKey, e => e.Settings);
            }
        }
    }
}