using System;
using System.Collections.Generic;
using System.Linq;
using ME.Contracts.Api.IncomingMessages;
using Newtonsoft.Json;
using NUnit.Framework;
using Service.Balances.Domain.Models;

namespace Service.Liquidity.Engine.Tests
{
    public class TestExample
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Console.WriteLine("Debug output");
            Assert.Pass();
        }

        [Test]
        public void Error_1()
        {
            var requestJson = "{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9\",\"BrokerId\":\"jetwallet\",\"AccountId\":\"LPFTX\",\"WalletId\":\"SP-LPFTX\",\"WalletVersion\":-1,\"AssetPairId\":\"BTCEUR\",\"Orders\":[{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-1\",\"Volume\":\"-1.00279999\",\"Price\":\"49231.19\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-2\",\"Volume\":\"-2.3389\",\"Price\":\"49255.21\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-3\",\"Volume\":\"-0.0682\",\"Price\":\"49258.21\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-4\",\"Volume\":\"-2.6379\",\"Price\":\"49348.3\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-5\",\"Volume\":\"-2.7589\",\"Price\":\"49397.35\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-6\",\"Volume\":\"-1.19130001\",\"Price\":\"49425.38\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-7\",\"Volume\":\"0.1026\",\"Price\":\"48992.95\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-8\",\"Volume\":\"2.4156\",\"Price\":\"48916.03\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-9\",\"Volume\":\"1.6546\",\"Price\":\"48893.05\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-10\",\"Volume\":\"0.0022\",\"Price\":\"48799.15\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-11\",\"Volume\":\"1.413\",\"Price\":\"48798.15\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-12\",\"Volume\":\"1.5683\",\"Price\":\"48797.15\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-13\",\"Volume\":\"4.2108\",\"Price\":\"48750.2\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-14\",\"Volume\":\"3.3673\",\"Price\":\"48719.23\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-15\",\"Volume\":\"3.529\",\"Price\":\"48675.27\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-16\",\"Volume\":\"2.24729487\",\"Price\":\"48672.27\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null}],\"CancelAllPreviousLimitOrders\":true,\"CancelMode\":1,\"Timestamp\":{\"Seconds\":1617187233,\"Nanos\":240757600},\"MessageId\":\"f42e9cebf2434321b5f212dd3a7715c9\"}";
            var balancesJson = "[{\"AssetId\":\"BTC\",\"Balance\":9.998,\"Reserve\":9.99799999,\"LastUpdate\":\"2021-03-31T10:31:15.688Z\",\"SequenceId\":308413},{\"AssetId\":\"EUR\",\"Balance\":1000159.97,\"Reserve\":950039.03,\"LastUpdate\":\"2021-03-31T10:31:15.688Z\",\"SequenceId\":308413},{\"AssetId\":\"USD\",\"Balance\":1000000.0,\"Reserve\":0.0,\"LastUpdate\":\"2021-03-30T10:39:25.398Z\",\"SequenceId\":308357}]";

            var request = JsonConvert.DeserializeObject<MultiLimitOrder>(requestJson);
            var balances = JsonConvert.DeserializeObject<List<WalletBalance>>(balancesJson);


            var quoteVolume = request.Orders
                .Select(e => new {Price = decimal.Parse(e.Price), Volume = decimal.Parse(e.Volume)})
                .Where(e => e.Volume > 0)
                .Sum(e => e.Price * e.Volume);

            Console.WriteLine($"Quote volume: {quoteVolume}");
            Console.WriteLine($"Quote balance: {balances.FirstOrDefault(e => e.AssetId == "EUR").Balance}");


        }

        [Test]
        public void Error_2()
        {
            var requestJson = "{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9\",\"BrokerId\":\"jetwallet\",\"AccountId\":\"LPFTX\",\"WalletId\":\"SP-LPFTX\",\"WalletVersion\":-1,\"AssetPairId\":\"BTCEUR\",\"Orders\":[{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-1\",\"Volume\":\"-1.00279999\",\"Price\":\"49231.19\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-2\",\"Volume\":\"-2.3389\",\"Price\":\"49255.21\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-3\",\"Volume\":\"-0.0682\",\"Price\":\"49258.21\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-4\",\"Volume\":\"-2.6379\",\"Price\":\"49348.3\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-5\",\"Volume\":\"-2.7589\",\"Price\":\"49397.35\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-6\",\"Volume\":\"-1.19130001\",\"Price\":\"49425.38\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-7\",\"Volume\":\"0.1026\",\"Price\":\"48992.95\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-8\",\"Volume\":\"2.4156\",\"Price\":\"48916.03\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-9\",\"Volume\":\"1.6546\",\"Price\":\"48893.05\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-10\",\"Volume\":\"0.0022\",\"Price\":\"48799.15\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-11\",\"Volume\":\"1.413\",\"Price\":\"48798.15\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-12\",\"Volume\":\"1.5683\",\"Price\":\"48797.15\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-13\",\"Volume\":\"4.2108\",\"Price\":\"48750.2\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-14\",\"Volume\":\"3.3673\",\"Price\":\"48719.23\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-15\",\"Volume\":\"3.529\",\"Price\":\"48675.27\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null},{\"Id\":\"f42e9cebf2434321b5f212dd3a7715c9-16\",\"Volume\":\"2.24729\",\"Price\":\"48672.27\",\"Fees\":[],\"OldId\":null,\"TimeInForce\":0,\"ExpiryTime\":null}],\"CancelAllPreviousLimitOrders\":true,\"CancelMode\":1,\"Timestamp\":{\"Seconds\":1617187233,\"Nanos\":240757600},\"MessageId\":\"f42e9cebf2434321b5f212dd3a7715c9\"}";
            var balancesJson = "[{\"AssetId\":\"BTC\",\"Balance\":9.998,\"Reserve\":9.99799999,\"LastUpdate\":\"2021-03-31T10:31:15.688Z\",\"SequenceId\":308413},{\"AssetId\":\"EUR\",\"Balance\":1000159.97,\"Reserve\":950039.03,\"LastUpdate\":\"2021-03-31T10:31:15.688Z\",\"SequenceId\":308413},{\"AssetId\":\"USD\",\"Balance\":1000000.0,\"Reserve\":0.0,\"LastUpdate\":\"2021-03-30T10:39:25.398Z\",\"SequenceId\":308357}]";

            var request = JsonConvert.DeserializeObject<MultiLimitOrder>(requestJson);
            var balances = JsonConvert.DeserializeObject<List<WalletBalance>>(balancesJson);


            var quoteVolume = request.Orders
                .Select(e => new { Price = decimal.Parse(e.Price), Volume = decimal.Parse(e.Volume) })
                .Where(e => e.Volume > 0)
                .Sum(e => e.Price * e.Volume);

            Console.WriteLine($"Quote volume: {quoteVolume}");
            Console.WriteLine($"Quote balance: {balances.FirstOrDefault(e => e.AssetId == "EUR").Balance}");


        }
    }
}
