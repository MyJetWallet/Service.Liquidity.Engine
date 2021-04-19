using System;
using Autofac;
using MyJetWallet.Domain.ExternalMarketApi;
using Service.Liquidity.Engine.Domain.Services.ExternalMarkets;

namespace Service.Liquidity.Engine.Modules
{
    public class ExternalExchangeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            foreach (var externalExchange in Program.Settings.ExternalExchange)
            {
                if (externalExchange.Value?.IsEnabled == true)
                {
                    Console.WriteLine($"External exchange: {externalExchange.Key}");
                    builder.RegisterExternalMarketClient(externalExchange.Value.GrpcUrl);
                }
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