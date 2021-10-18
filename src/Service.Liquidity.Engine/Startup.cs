using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using MyJetWallet.Sdk.GrpcMetrics;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.Service;
using Prometheus;
using ProtoBuf.Grpc.Server;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.GrpcServices;
using Service.Liquidity.Engine.Modules;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;

namespace Service.Liquidity.Engine
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.BindCodeFirstGrpc();

            services.AddHostedService<ApplicationLifetimeManager>();

            services.AddMyTelemetry("SP-", Program.Settings.ZipkinUrl);

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseMetricServer();

            app.BindServicesTree(Assembly.GetExecutingAssembly());

            app.BindIsAlive();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcSchema<OrderBookManagerGrpc, IOrderBookManagerGrpc>();
                endpoints.MapGrpcSchema<LpWalletManagerGrpc, ILpWalletManagerGrpc>();
                endpoints.MapGrpcSchema<MarketMakerSettingsManagerGrpc, IMarketMakerSettingsManagerGrpc>();
                endpoints.MapGrpcSchema<WalletPortfolioGrpc, IWalletPortfolioGrpc>();
                endpoints.MapGrpcSchema<ExternalMarketsGrpc, IExternalMarketsGrpc>();
                endpoints.MapGrpcSchema<HedgeSettingsManagerGrpc, IHedgeSettingsManagerGrpc>();
                

                endpoints.MapGrpcSchemaRegistry();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            Mathematics.AccuracyToNormalizeDouble = Program.Settings.AccuracyToNormalizeDouble;
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule<ExternalExchangeModule>();
            builder.RegisterModule<HedgeServiceModule>();
            builder.RegisterModule<ServiceBusModule>();
        }
    }
}
