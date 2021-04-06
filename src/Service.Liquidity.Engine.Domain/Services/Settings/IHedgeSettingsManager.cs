using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Settings;

namespace Service.Liquidity.Engine.Domain.Services.Settings
{
    public interface IHedgeSettingsManager
    {
        HedgeSettings GetGlobalHedgeSettings();

        Task UpdateSettingsAsync(HedgeSettings settings);
    }
}