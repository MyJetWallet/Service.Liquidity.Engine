using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.Engine.Domain.Models.Portfolio;
using Service.Liquidity.Engine.Grpc.Models;

namespace Service.Liquidity.Engine.Grpc
{
    [ServiceContract]
    public interface IWalletPortfolioGrpc
    {
        [OperationContract]
        Task<GrpcList<PositionPortfolio>> GetPortfolioAsync();
        
    }
    
}