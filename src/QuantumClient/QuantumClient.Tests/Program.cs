using Quantum;
using System;

namespace QuantumClient.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client("https//api.quantum.exchange", "apikey", "secret");
            
            var orderBook = client.GetOrderBook("btc","cad");

            foreach(var order in orderBook.book)
            {
                Console.WriteLine("{0} {1} BTC at {2} CAD", order.action, order.amount, order.limit_price);
            }

            var balances = client.GetBalance();

            foreach(var entry in balances)
            {
                Console.WriteLine("{0}: {1}", entry.id, entry.balance);
            }

            var id = client.PlaceLimitOrder("buy", 0.001M, "btc", "cad", 8070.77M);

            Console.WriteLine("Got order id: {0}", id);

            var success = client.CancelOrder(id);

            Console.WriteLine("Order cancelled: {0}", success);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
