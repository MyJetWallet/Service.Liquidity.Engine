using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.NoSql;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public class HedgeSettingsManager : IHedgeSettingsManager
    {
        private readonly ILogger<MarketMakerSettingsManager> _logger;
        private readonly IMyNoSqlServerDataWriter<SettingsHedgeGlobalNoSql> _hedgeSettingsDataWriter;

        private HedgeSettings _globalSettings = new() {Mode = EngineMode.Disabled};

        private readonly object _sync = new();

        public HedgeSettingsManager(
            ILogger<MarketMakerSettingsManager> logger,
            IMyNoSqlServerDataWriter<SettingsHedgeGlobalNoSql> hedgeSettingsDataWriter)
        {
            _logger = logger;
            _hedgeSettingsDataWriter = hedgeSettingsDataWriter;
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
        }
    }
}