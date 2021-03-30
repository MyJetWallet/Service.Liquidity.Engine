using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Service.Liquidity.Engine.Grpc.Models
{
    [DataContract]
    public class GrpcResponseWithData<T>
    {
        [DataMember(Order = 1)] public T Data { get; set; }

        public static GrpcResponseWithData<T> Create(T data)
        {
            return new GrpcResponseWithData<T>() {Data = data};
        }

        public static Task<GrpcResponseWithData<T>> CreateTask(T data)
        {
            return Task.FromResult(new GrpcResponseWithData<T>() { Data = data });
        }
    }
}