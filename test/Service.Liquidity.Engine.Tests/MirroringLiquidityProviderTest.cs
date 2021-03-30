using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MyJetWallet.MatchingEngine.Grpc.Api;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.OrderBooks;
using Service.Liquidity.Engine.Domain.Models.Settings;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.Services.MarketMakers;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Settings;
using Service.Liquidity.Engine.Domain.Services.Wallets;

namespace Service.Liquidity.Engine.Tests
{
    public class MirroringLiquidityProviderTest
    {
        private MirroringLiquidityProvider _engine;
        private ILoggerFactory _loggerFactory;

        private OrderBookManagerMock _orderBookManager;
        private MarketMakerSettingsAccessorMock _settingsMock;
        private LpWalletManagerMock _walletManager;
        private TradingServiceClientMock _tradingService;

        [SetUp]
        public void Setup()
        {
            _orderBookManager = new();
            _settingsMock = new();
            _walletManager = new();
            _tradingService = new ();

            _loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));

            _engine = new MirroringLiquidityProvider(
                _loggerFactory.CreateLogger<MirroringLiquidityProvider>(),
                _orderBookManager,
                _settingsMock,
                _walletManager,
                _tradingService
                );

        }

        [Test]
        public void Create_Engine()
        {
            Assert.NotNull(_engine);

            _loggerFactory.CreateLogger("Test").LogInformation("Debug output");

            Console.WriteLine("Debug output");
            Assert.Pass();
        }
    }

    public class OrderBookManagerMock: IOrderBookManager
    {
        public Dictionary<(string, string), LeOrderBook> Data { get; set; } = new();

        public LeOrderBook GetOrderBook(string symbol, string source)
        {
            Data.TryGetValue((symbol, source), out var book);
            return book;
        }

        public Dictionary<string, List<string>> GetSourcesAndSymbols()
        {
            throw new NotImplementedException();
        }

        public List<string> GetSymbols(string source)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSourcesWithSymbol(string symbol)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSources()
        {
            throw new NotImplementedException();
        }
    }

    public class MarketMakerSettingsAccessorMock: IMarketMakerSettingsAccessor
    {
        public MarketMakerSettings MmSettings { get; set; } = new(EngineMode.Disabled);

        public List<MirroringLiquiditySettings> MlSettings { get; set; } = new();

        public MarketMakerSettings GetMarketMakerSettings()
        {
            return MmSettings;
        }

        public List<MirroringLiquiditySettings> GetMirroringLiquiditySettingsList()
        {
            return MlSettings;
        }
    }

    public class LpWalletManagerMock: ILpWalletManager
    {
        public Dictionary<string, Dictionary<string, WalletBalance>> Data { get; set; }= new();

        public List<WalletBalance> GetBalances(string walletName)
        {
            if (Data.TryGetValue(walletName, out var data))
            {
                return data.Values.ToList();
            }

            return new List<WalletBalance>();
        }

        public Task AddWalletAsync(LpWallet wallet)
        {
            throw new NotImplementedException();
        }

        public Task RemoveWalletAsync(string name)
        {
            throw new NotImplementedException();
        }

        public List<LpWallet> GetAll()
        {
            throw new NotImplementedException();
        }
    }

    public class TradingServiceClientMock : ITradingServiceClient
    {
        public List<MultiLimitOrder> CallList { get; set; } = new();

        public Func<MultiLimitOrder, MultiLimitOrderResponse> MultiLimitOrderCallback { get; set; }

        public MarketOrderResponse MarketOrder(MarketOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<MarketOrderResponse> MarketOrderAsync(MarketOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public LimitOrderResponse LimitOrder(LimitOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<LimitOrderResponse> LimitOrderAsync(LimitOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public LimitOrderCancelResponse CancelLimitOrder(LimitOrderCancel request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<LimitOrderCancelResponse> CancelLimitOrderAsync(LimitOrderCancel request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public MultiLimitOrderResponse MultiLimitOrder(MultiLimitOrder request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CallList.Add(request);

            if (MultiLimitOrderCallback != null)
            {
                return MultiLimitOrderCallback.Invoke(request);
            }

            var resp = new MultiLimitOrderResponse()
            {
                Id = request.Id,
                MessageId = request.MessageId,
                AssetPairId = request.AssetPairId,
                Status = Status.Ok,
                WalletVersion = 0
            };

            foreach (var order in request.Orders)
            {
                resp.Statuses.Add(new MultiLimitOrderResponse.Types.OrderStatus()
                {
                    Id = order.Id,
                    MatchingEngineId = order.Id,
                    Price = order.Price,
                    Status = Status.Ok,
                    Volume = order.Volume
                });
            }

            return resp;
        }

        public Task<MultiLimitOrderResponse> MultiLimitOrderAsync(MultiLimitOrder request, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(MultiLimitOrder(request));
        }
    }
}