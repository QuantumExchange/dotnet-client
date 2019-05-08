# .NET client for the Quantum Exchange

## C# Example

```csharp
var client = new Client("https://api.quantum.exchange", "apikey", "secret");

var orderBook = client.GetOrderBook("btc","cad");

foreach(var order in orderBook.book){
    Console.WriteLine("{0} {1} BTC at {2} CAD", order.action, order.amount, order.limit_price);
}
```
