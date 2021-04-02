using System;
using Autofac;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets.SimulationFtx;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Jobs;
using Service.Simulation.FTX.Client;

namespace Service.Liquidity.Engine.Modules
{
    public class HedgeServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ExternalMarketManager>()
                .As<IExternalMarketManager>()
                .SingleInstance();


            var ftxSimulation = Environment.GetEnvironmentVariable("SIMULATION_FTX");
            if (!string.IsNullOrEmpty(ftxSimulation))
            {
                builder.RegisterSimulationFtxClient(ftxSimulation);

                builder
                    .RegisterType<SimulationFtxExternalMarket>()
                    .As<IExternalMarket>()
                    .SingleInstance();
            }

            builder
                .RegisterType<PortfolioMyNoSqlRepository>()
                .As<IPortfolioRepository>()
                .SingleInstance();


            builder
                .RegisterType<PortfolioManager>()
                .As<IPortfolioManager>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<InternalTradeReaderJob>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}