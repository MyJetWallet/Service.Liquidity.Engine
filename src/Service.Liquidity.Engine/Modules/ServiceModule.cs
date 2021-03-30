using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;

namespace Service.Liquidity.Engine.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<OrderBookManager>()
                .As<IOrderBookManager>()
                .SingleInstance();
        }
    }
}