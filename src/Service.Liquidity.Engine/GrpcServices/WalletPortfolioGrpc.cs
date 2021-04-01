using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Domain.Services.Portfolio;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.GrpcServices
{
    public class WalletPortfolioGrpc: IWalletPortfolioGrpc
    {
        private readonly IPortfolioManager _manager;

        public WalletPortfolioGrpc(IPortfolioManager manager)
        {
            _manager = manager;
        }

        public Task<WalletPortfolio> GetPortfolioAsync(WalletNameRequest request)
        {
            return _manager.GetPortfolioByWalletName(request.WalletName);
        }
    }
}