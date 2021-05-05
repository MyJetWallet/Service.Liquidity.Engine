using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Services.Settings;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class HedgeSettingsManagerMock: IHedgeSettingsManager
    {
        public HedgeSettings GlobalSettings = new HedgeSettings() {Mode = EngineMode.Disabled};

        public HedgeSettings GetGlobalHedgeSettings()
        {
            return GlobalSettings;
        }

        public Task UpdateSettingsAsync(HedgeSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}