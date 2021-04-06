using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.Settings;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class HedgeSettingsManagerMock: IHedgeSettingsManager, IHedgeInstrumentSettingsManager
    {
        public HedgeSettings GlobalSettings = new HedgeSettings() {Mode = EngineMode.Disabled};
        public Dictionary<string, HedgeInstrumentSettings> InstrumentSettings = new();

        public HedgeSettings GetGlobalHedgeSettings()
        {
            return GlobalSettings;
        }

        public Task UpdateSettingsAsync(HedgeSettings settings)
        {
            throw new System.NotImplementedException();
        }

        public List<HedgeInstrumentSettings> GetHedgeInstrumentSettingsList()
        {
            return InstrumentSettings.Values.ToList();
        }

        public Task AddOrUpdateSettings(HedgeInstrumentSettings settings)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveSettings(string symbol, string walletId)
        {
            throw new System.NotImplementedException();
        }

        public HedgeInstrumentSettings GetHedgeInstrumentSettings(string symbol, string walletId)
        {
            return InstrumentSettings.Values.FirstOrDefault(e => e.WalletId == walletId && e.InstrumentSymbol == symbol);
        }
    }
}