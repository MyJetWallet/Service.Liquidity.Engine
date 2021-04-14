using Autofac;
using MyJetWallet.Domain.ExternalMarketApi;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;

namespace Service.Liquidity.Engine.Modules
{
    public class ExternalExchangeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (Program.Settings.FtxSimulateIsEnabled)
            {
                builder.RegisterExternalMarketClient(Program.Settings.FtxSimulateExchangeGrpcUrl);
            }

            if (Program.Settings.FtxIsEnabled)
            {
                builder.RegisterExternalMarketClient(Program.Settings.FtxExchangeGrpcUrl);
            }

            builder
                .RegisterType<ExternalBalanceCacheManager>()
                .As<IExternalBalanceCacheManager>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}