using Autofac;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;
using Service.Liquidity.Engine.Domain.Services.Hedger;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Jobs;

namespace Service.Liquidity.Engine.Modules
{
    public class HedgeServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ExternalMarketManager>()
                .As<IExternalMarketManager>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<PortfolioMyNoSqlRepository>()
                .As<IPortfolioRepository>()
                .SingleInstance();


            builder
                .RegisterType<PortfolioManager>()
                .As<IPortfolioManager>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<InternalTradeReaderJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<PortfolioReport>()
                .As<IPortfolioReport>()
                .SingleInstance();

            builder
                .RegisterType<HedgeService>()
                .As<IHedgeService>()
                .SingleInstance();

            builder
                .RegisterType<HedgeSettingsManager>()
                .AsSelf()
                .As<IHedgeSettingsManager>()
                .As<IHedgeInstrumentSettingsManager>()
                .SingleInstance();
        }
    }
}