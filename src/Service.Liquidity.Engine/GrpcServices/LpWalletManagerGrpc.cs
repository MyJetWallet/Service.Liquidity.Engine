using System.Threading.Tasks;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Service.Liquidity.Engine.Grpc;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.GrpcServices
{
    public class LpWalletManagerGrpc : ILpWalletManagerGrpc
    {
        private readonly ILpWalletManager _manager;

        public LpWalletManagerGrpc(ILpWalletManager manager)
        {
            _manager = manager;
        }

        public Task<GrpcResponseWithData<GrpcList<WalletBalance>>> GetBalancesAsync(WalletNameRequest request)
        {
            var balances = _manager.GetBalances(request.WalletName);

            return GrpcResponseWithData<GrpcList<WalletBalance>>.CreateTask(GrpcList<WalletBalance>.Create(balances));
        }

        public Task AddWalletAsync(LpWallet wallet)
        {
            return _manager.AddWalletAsync(wallet);
        }

        public Task RemoveWalletAsync(WalletNameRequest request)
        {
            return _manager.RemoveWalletAsync(request.WalletName);
        }

        public Task<GrpcResponseWithData<GrpcList<LpWallet>>> GetAllAsync()
        {
            var data = _manager.GetAll();

            return GrpcResponseWithData<GrpcList<LpWallet>>.CreateTask(GrpcList<LpWallet>.Create(data));
        }
    }
}