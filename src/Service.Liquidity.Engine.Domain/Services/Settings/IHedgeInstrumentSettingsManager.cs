using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public interface IHedgeInstrumentSettingsManager
    {
        List<HedgeInstrumentSettings> GetHedgeInstrumentSettingsList();

        Task AddOrUpdateSettings(HedgeInstrumentSettings settings);

        Task RemoveSettings(string symbol, string walletId);
        HedgeInstrumentSettings GetHedgeInstrumentSettings(string symbol, string walletId);
    }
}