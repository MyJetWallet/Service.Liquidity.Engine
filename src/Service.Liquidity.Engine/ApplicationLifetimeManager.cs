using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using MyServiceBus.TcpClient;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Jobs;

namespace Service.Liquidity.Engine
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlTcpClient _myNoSqlClient;
        private readonly MyServiceBusTcpClient _myServiceBusTcpClient;
        private readonly MarketMakerJob _marketMakerJob;
        private readonly HedgeSettingsManager _hedgeSettingsManager;
        private readonly MarketMakerSettingsManager _marketMakerSettingsManager;
        private readonly PortfolioManager _portfolioManager;

        public ApplicationLifetimeManager(
            IHostApplicationLifetime appLifetime, 
            ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlTcpClient myNoSqlClient,
            MyServiceBusTcpClient myServiceBusTcpClient,
            MarketMakerJob marketMakerJob,
            HedgeSettingsManager hedgeSettingsManager,
            MarketMakerSettingsManager marketMakerSettingsManager,
            PortfolioManager portfolioManager)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlClient = myNoSqlClient;
            _myServiceBusTcpClient = myServiceBusTcpClient;
            _marketMakerJob = marketMakerJob;
            _hedgeSettingsManager = hedgeSettingsManager;
            _marketMakerSettingsManager = marketMakerSettingsManager;
            _portfolioManager = portfolioManager;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _hedgeSettingsManager.Start();
            _marketMakerSettingsManager.Start();
            _portfolioManager.Start();


            _myNoSqlClient.Start();
            _myServiceBusTcpClient.Start();


            _marketMakerJob.Start();
        }

        protected override void OnStopping()
        {
            _marketMakerJob.Stop();

            _logger.LogInformation("OnStopping has been called.");
            _myNoSqlClient.Stop();

            try
            {
                _myServiceBusTcpClient.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception on MyServiceBusTcpClient.Stop: {ex}");
            }
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
