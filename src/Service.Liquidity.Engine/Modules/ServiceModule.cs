using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.MatchingEngine.Grpc;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.AssetsDictionary.Client;
using Service.Balances.Client;
using Service.Balances.Grpc;
using Service.Liquidity.Engine.Domain.NoSql;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Service.Liquidity.Engine.Jobs;

namespace Service.Liquidity.Engine.Modules
{
    public class ServiceModule: Module
    {
        private MyNoSqlTcpClient _myNoSqlClient;

        protected override void Load(ContainerBuilder builder)
        {
            RegisterMyNoSqlTcpClient(builder);

            builder
                .RegisterType<OrderBookManager>()
                .As<IOrderBookManager>()
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
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<MirroringLiquidityProvider>()
                .As<IMarketMaker>()
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

            
            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, _myNoSqlClient);
            builder.RegisterMatchingEngineGrpcClient(tradingServiceGrpcUrl: Program.Settings.MatchingEngineTradingServiceGrpcUrl);
            builder.RegisterAssetsDictionaryClients(_myNoSqlClient);



            


            RegisterMyNoSqlWriter<LpWalletNoSql>(builder, LpWalletNoSql.TableName);
            RegisterMyNoSqlWriter<SettingsMarketMakerNoSql>(builder, SettingsMarketMakerNoSql.TableName);
            RegisterMyNoSqlWriter<SettingsMirroringLiquidityNoSql>(builder, SettingsMirroringLiquidityNoSql.TableName);
            RegisterMyNoSqlWriter<WalletPortfolioNoSql>(builder, WalletPortfolioNoSql.TableName);
            



        }

        private void RegisterMyNoSqlTcpClient(ContainerBuilder builder)
        {
            _myNoSqlClient = new MyNoSqlTcpClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder
                .RegisterInstance(_myNoSqlClient)
                .AsSelf()
                .SingleInstance();
        }

        private void RegisterMyNoSqlWriter<TEntity>(ContainerBuilder builder, string table)
            where TEntity : IMyNoSqlDbEntity, new()
        {
            builder.Register(ctx => new MyNoSqlServer.DataWriter.MyNoSqlServerDataWriter<TEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), table, true))
                .As<IMyNoSqlServerDataWriter<TEntity>>()
                .SingleInstance();

        }
    }
}