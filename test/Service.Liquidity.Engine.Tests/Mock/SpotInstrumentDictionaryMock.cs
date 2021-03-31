using System;
using System.Collections.Generic;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class SpotInstrumentDictionaryMock : ISpotInstrumentDictionaryClient
    {
        public Dictionary<string, SpotInstrument> Data = new();

        public ISpotInstrument GetSpotInstrumentById(ISpotInstrumentIdentity spotInstrumentId)
        {
            if (!Data.TryGetValue(spotInstrumentId.Symbol, out var instrument))
                return null;

            return instrument;
        }

        public IReadOnlyList<ISpotInstrument> GetSpotInstrumentByBroker(IJetBrokerIdentity brokerId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ISpotInstrument> GetSpotInstrumentByBrand(IJetBrandIdentity brandId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ISpotInstrument> GetAllSpotInstruments()
        {
            throw new NotImplementedException();
        }

        public event Action OnChanged;
    }
}