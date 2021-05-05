using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Service.Liquidity.Engine.Tests
{
    public class AggregateLiquidityProviderTest : AggregateLiquidityProviderTestBase
    {
        [Test]
        public async Task ValidateMaxSideVolume()
        {
            SetupEnvironment_1();

            _settingsMock.LpSettings.ForEach(e => e.LpSources.ForEach(s =>
            {
                s.MaxSellSideVolume = 1.1;
                s.MaxBuySideVolume = 1.2;
            }));

            await _engine.RefreshOrders();


            _tradingService.CallList.Should().HaveCount(1);

            _tradingService.CallList[0].Orders.Select(e => decimal.Parse(e.Volume)).Where(e => e < 0).Sum().Should().Be(-1.1m, "Sell side limited");
            _tradingService.CallList[0].Orders.Select(e => decimal.Parse(e.Volume)).Where(e => e > 0).Sum().Should().Be(1.2m, "Buy side limited");
        }
    }
}