using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Liquidity.Engine.ExchangeConnectors.Ftx;

namespace Service.Liquidity.Engine
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly FtxOrderBookSource _ftxOrderBookSource;

        public ApplicationLifetimeManager(
            IHostApplicationLifetime appLifetime, 
            ILogger<ApplicationLifetimeManager> logger,
            FtxOrderBookSource ftxOrderBookSource)
            : base(appLifetime)
        {
            _logger = logger;
            _ftxOrderBookSource = ftxOrderBookSource;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _ftxOrderBookSource.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _ftxOrderBookSource.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
