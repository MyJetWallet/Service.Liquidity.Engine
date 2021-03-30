using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using Service.Liquidity.Engine.ExchangeConnectors.Ftx;
using Service.Liquidity.Engine.Jobs;

namespace Service.Liquidity.Engine
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlTcpClient _myNoSqlClient;
        private readonly FtxOrderBookSource _ftxOrderBookSource;
        private readonly MarketMakerJob _marketMakerJob;

        public ApplicationLifetimeManager(
            IHostApplicationLifetime appLifetime, 
            ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlTcpClient myNoSqlClient,
            FtxOrderBookSource ftxOrderBookSource,
            MarketMakerJob marketMakerJob)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlClient = myNoSqlClient;
            _ftxOrderBookSource = ftxOrderBookSource;
            _marketMakerJob = marketMakerJob;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _myNoSqlClient.Start();
            _ftxOrderBookSource.Start();


            _marketMakerJob.Start();
        }

        protected override void OnStopping()
        {
            _marketMakerJob.Stop();

            _logger.LogInformation("OnStopping has been called.");
            _myNoSqlClient.Stop();
            _ftxOrderBookSource.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
