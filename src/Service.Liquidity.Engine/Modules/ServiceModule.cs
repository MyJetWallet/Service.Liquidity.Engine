using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.AssetsDictionary.Client;
using Service.Balances.Client;
using Service.Liquidity.Engine.Domain.Models.NoSql;
using Service.Liquidity.Engine.Domain.NoSql;
using Service.Liquidity.Engine.Domain.Services.LiquidityProvider;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Service.Liquidity.Engine.Jobs;
using Service.MatchingEngine.Api.Client;

namespace Service.Liquidity.Engine.Modules
{
    public class ServiceModule: Module
    {

        protected override void Load(ContainerBuilder builder)
        {

            builder
                .RegisterType<OrderBookManager>()
                .As<IOrderBookManager>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<LpWalletManager>()
                .As<ILpWalletManager>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<MarketMakerSettingsManager>()
                .As<IMarketMakerSettingsManager>()
                .As<IMarketMakerSettingsAccessor>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<AggregateLiquidityProvider>()
                .As<IMarketMaker>()
                .As<IAggregateLiquidityProviderOrders>()
                .AutoActivate()
                .SingleInstance();

            

            builder
                .RegisterType<OrderIdGenerator>()
                .As<IOrderIdGenerator>()
                .SingleInstance();

            builder
                .RegisterType<MarketMakerJob>()
                .AsSelf()
                .SingleInstance();

            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            
            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, myNoSqlClient);
            builder.RegisterMatchingEngineApiClient(Program.Settings.MatchingEngineApiGrpcServiceUrl);
            builder.RegisterAssetsDictionaryClients(myNoSqlClient);

            builder.RegisterMyNoSqlWriter<LpWalletNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), LpWalletNoSql.TableName);
            builder.RegisterMyNoSqlWriter<SettingsMarketMakerNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), SettingsMarketMakerNoSql.TableName);
            builder.RegisterMyNoSqlWriter<PositionPortfolioNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), PositionPortfolioNoSql.TableName);
            builder.RegisterMyNoSqlWriter<SettingsHedgeGlobalNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), SettingsHedgeGlobalNoSql.TableName);
            builder.RegisterMyNoSqlWriter<SettingsLiquidityProviderInstrumentNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), SettingsLiquidityProviderInstrumentNoSql.TableName);
        }
    }
}