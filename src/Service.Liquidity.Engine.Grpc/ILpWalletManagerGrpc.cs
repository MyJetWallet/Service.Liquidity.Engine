using System.ServiceModel;
using System.Threading.Tasks;
using Service.Balances.Domain.Models;
using Service.Liquidity.Engine.Domain.Models.Wallets;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.Grpc
{
    [ServiceContract]
    public interface ILpWalletManagerGrpc
    {
        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<WalletBalance>>> GetBalancesAsync(WalletNameRequest request);

        [OperationContract]
        Task AddWalletAsync(LpWallet wallet);

        [OperationContract]
        Task RemoveWalletAsync(WalletNameRequest request);

        [OperationContract]
        Task<GrpcResponseWithData<GrpcList<LpWallet>>> GetAllAsync();
    }
}