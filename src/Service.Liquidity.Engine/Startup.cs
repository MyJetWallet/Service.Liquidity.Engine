using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using MyJetWallet.Sdk.GrpcMetrics;
using MyJetWallet.Sdk.GrpcSchema;
using Prometheus;
using ProtoBuf.Grpc.Server;
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
            services.AddCodeFirstGrpc(options =>
            {
                options.Interceptors.Add<PrometheusMetricsInterceptor>();
                options.BindMetricsInterceptors();
            });

            services.AddHostedService<ApplicationLifetimeManager>();
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
                endpoints.MapGrpcSchema<HelloService, IHelloService>();
                endpoints.MapGrpcSchema<OrderBookManagerGrpc, IOrderBookManagerGrpc>();
                endpoints.MapGrpcSchema<LpWalletManagerGrpc, ILpWalletManagerGrpc>();
                endpoints.MapGrpcSchema<MarketMakerSettingsManagerGrpc, IMarketMakerSettingsManagerGrpc>();

                endpoints.MapGrpcSchemaRegistry();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule<ExternalExchangeModule>();
            
        }
    }
}
