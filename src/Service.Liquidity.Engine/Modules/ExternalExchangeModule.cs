using System.Collections.Generic;
using System.Linq;
using Autofac;
using Service.Liquidity.Engine.Domain.Models;
using Service.Liquidity.Engine.Domain.Services.OrderBooks;
using Service.Liquidity.Engine.Domain.Services.Wallets;
using Service.Liquidity.Engine.ExchangeConnectors.Ftx;

namespace Service.Liquidity.Engine.Modules
{
    public class ExternalExchangeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (Program.Settings.FtxIsEnabled)
            {
                List<string> ftxInstrumentList = Program.Settings.FtxInstrumentsOriginalSymbolToSymbol
                    .Split(';')
                    .ToList();

                builder
                    .RegisterType<FtxOrderBookSource>()
                    .WithParameter("symbolList", ftxInstrumentList)
                    .AsSelf()
                    .As<IOrderBookSource>()
                    .SingleInstance();
                
            }
        }
    }
}