﻿using Quantum;
using System;

namespace QuantumClient.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client("https://api.quantum.exchange", "apikey", "secret");
            
            var orderBook = client.GetOrderBook("eth","usdt");

            foreach(var order in orderBook.asks)
            {
                Console.WriteLine("{0} {1} BTC at {2} CAD", "SELL", order.amount, order.price);
            }

            foreach (var order in orderBook.bids)
            {
                Console.WriteLine("{0} {1} BTC at {2} CAD", "BUY", order.amount, order.price);
            }

            var balances = client.GetBalance();

            foreach(var entry in balances)
            {
                Console.WriteLine("{0}: {1}", entry.id, entry.balance);
            }

            var id = client.PlaceLimitOrder("buy", 0.001M, "btc", "cad", 8070.77M);

            Console.WriteLine("Got order id: {0}", id);

            var success = client.CancelOrder(id, "eth", "usdt");

            Console.WriteLine("Order cancelled: {0}", success);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
