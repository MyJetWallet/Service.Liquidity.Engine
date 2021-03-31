using System;
using System.Collections.Generic;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;

namespace Service.Liquidity.Engine.Tests.Mock
{
    public class AssetsDictionaryMock : IAssetsDictionaryClient
    {
        public Dictionary<string, Asset> Data = new();

        public IAsset GetAssetById(IAssetIdentity assetId)
        {
            if (!Data.TryGetValue(assetId.Symbol, out var asset))
                return null;

            return asset;
        }

        public IReadOnlyList<IAsset> GetAssetsByBroker(IJetBrokerIdentity brokerId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IAsset> GetAssetsByBrand(IJetBrandIdentity brandId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IAsset> GetAllAssets()
        {
            throw new NotImplementedException();
        }

        public event Action OnChanged;
    }
}