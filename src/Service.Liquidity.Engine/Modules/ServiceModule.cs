﻿using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.Balances.Client;
using Service.Balances.Grpc;
using Service.Liquidity.Engine.Domain.NoSql;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Wallets;

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

            
            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, _myNoSqlClient);


            RegisterMyNoSqlWriter<LpWalletNoSql>(builder, LpWalletNoSql.TableName);
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