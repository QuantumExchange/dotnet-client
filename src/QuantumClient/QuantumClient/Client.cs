using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RestSharp;

namespace Quantum
{
    public class Client
    {

        string serverUrl = "https://api.quantum.exchange";
        string key;
        string secret;

        public Client() { }

        public Client(string serverUrl, string key, string secret)
        {
            this.serverUrl = serverUrl;
            this.key = key;
            this.secret = secret;

        }

        public Client(string key, string secret)
        {
            this.key = key;
            this.secret = secret;
        }

        private string ByteToString(byte[] buff)
        {
            var sbinary = new StringBuilder();
            foreach (var b in buff)
            {
                sbinary.Append(b.ToString("x2"));
            }

            return sbinary.ToString();
        }

        private string HMAC(string key, string message)
        {
            var keyByte = Encoding.UTF8.GetBytes(key);
            using (var hmacsha512 = new HMACSHA256(keyByte))
            {
                hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(message));
                return ByteToString(hmacsha512.Hash).ToLowerInvariant();
            }
        }

        private long GetNonce()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (DateTime.UtcNow - epoch).Ticks / 10L;
        }

        private IRestResponse Call(Method method, string endPoint, string data, bool priv)
        {
            var client = new RestClient(serverUrl);
            var request = new RestRequest(endPoint, method);

            if (priv)
            {
                request.AddHeader("QUANTUM_API_KEY", key);
                var nonce = GetNonce();
                var signature = HMAC(secret, nonce.ToString() + method.ToString() + endPoint + data);
                request.AddHeader("QUANTUM_API_SIGNATURE", signature);
                request.AddHeader("QUANTUM_API_NONCE", nonce.ToString());
            }

            if (!string.IsNullOrEmpty(data))
            {
                request.AddParameter("text/json", data, ParameterType.RequestBody);
            }

            var result = client.Execute(request);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(result.Content);
            }

            return result;
        }

        public GetOrderBookResponse GetOrderBook(string asset, string currency)
        {
            var result = Call(Method.GET, "/v1/order_book/" + asset + "/" + currency, null, false);
            return JsonConvert.DeserializeObject<GetOrderBookResponse>(result.Content);
        }

        public Balance[] GetBalance()
        {
            var result = Call(Method.GET, "/v1/balance", null, true);
            return JsonConvert.DeserializeObject<Balance[]>(result.Content);
        }

        public OpenOrderItem[] GetOpenOrders(string asset, string currency)
        {
            var result = Call(Method.GET, "/v1/orders/" + asset + "/" + currency + "/open", null, true);
            return JsonConvert.DeserializeObject<OpenOrderItem[]>(result.Content);
        }

        public StopLossOrderItem[] GetTakeProfitOrders(string asset, string currency)
        {
            var result = Call(Method.GET, "/v1/orders/" + asset + "/" + currency + "/take_profit", null, true);
            return JsonConvert.DeserializeObject<StopLossOrderItem[]>(result.Content);
        }

        public StopLossOrderItem[] GetStopLossOrders(string asset, string currency)
        {
            var result = Call(Method.GET, "/v1/orders/" + asset + "/" + currency + "/stop_loss", null, true);
            return JsonConvert.DeserializeObject<StopLossOrderItem[]>(result.Content);
        }

        public string PlaceLimitOrder(string action, decimal amount, string asset, string currency, decimal price, string[] options)
        {
            var data = new PlaceOrderRequest()
            {
                action = action,
                amount = amount,
                asset = asset,
                currency = currency,
                price = price,
                type = "limit",
                options = options
            };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);

            return obj.id;
        }

        public string PlaceLimitOrder(string action, decimal amount, string asset, string currency, decimal price)
        {
            return PlaceLimitOrder(action, amount, asset, currency, price, new string[] { });
        }

        public string PlaceMarketOrder(string action, decimal amount, string asset, string currency)
        {
            var data = new PlaceOrderRequest()
            {
                action = action,
                amount = amount,
                asset = asset,
                currency = currency,
                type = "market"
            };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);
            return obj.id;
        }

        public string PlaceQuotedMarketOrder(string action, decimal quote_amount, string asset, string currency)
        {
            var data = new PlaceOrderRequest()
            {
                action = action,
                quote_amount = quote_amount ,
                asset = asset,
                currency = currency,
                type = "market"
            };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);
            return obj.id;
        }

        public string PlaceTakeProfitLimitOrder(string action, decimal amount, string asset, string currency, decimal price, decimal stop_price, string[] options)
        {
            var data = new PlaceOrderRequest()
            {
                action = action,
                amount = amount,
                asset = asset,
                currency = currency,
                price = price,
                stop_price = stop_price,
                type = "take-profit-limit",
                options = options
            };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);

            return obj.id;
        }

        public string PlaceTakeProfitLimitOrder(string action, decimal amount, string asset, string currency, decimal price, decimal stop_price)
        {
            return PlaceTakeProfitLimitOrder(action, amount, asset, currency, price, stop_price, new string[] { });
        }

        public string PlaceTakeProfitMarketOrder(string action, decimal amount, string asset, string currency, decimal price, decimal stop_price)
        {
            var data = new PlaceOrderRequest()
            {
                action = action
                ,
                amount = amount
                ,
                asset = asset
                ,
                currency = currency
                ,
                price = price
                ,
                stop_price = stop_price
                ,
                type = "take-profit-market"
            };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);

            return obj.id;
        }

        public string PlaceStopLossLimitOrder(string action, decimal amount, string asset, string currency, decimal price, decimal stop_price, string[] options)
        {
            var data = new PlaceOrderRequest()
            {
                action = action,
                amount = amount,
                asset = asset,
                currency = currency,
                price = price,
                stop_price = stop_price,
                type = "stop-loss-limit"
                ,options = options
            };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);

            return obj.id;
        }

        public string PlaceStopLossLimitOrder(string action, decimal amount, string asset, string currency, decimal price, decimal stop_price)
        {
            return PlaceStopLossLimitOrder(action, amount, asset, currency, price, stop_price, new string[] { });
        }

        public string PlaceStopLossMarketOrder(string action, decimal amount, string asset, string currency, decimal price, decimal stop_price)
        {
            var data = new PlaceOrderRequest()
            {
                action = action,
                amount = amount,
                asset = asset,
                currency = currency,
                price = price,
                type = "stop-loss-market",
                stop_price = stop_price
            };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);

            return obj.id;
        }

        public bool CancelOrder(string orderId, string asset, string currency)
        {
            var data = new CanceOrderRequest() { id = orderId, asset = asset, currency = currency };
            var result = Call(Method.POST, "/v1/order/cancel", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<CancelOrderResponse>(result.Content);

            return obj.success;
        }

        public bool CancelAllOrders(string asset, string currency)
        {
            var data = new CancelAllOrdersRequest() { asset = asset, currency = currency };
            var result = Call(Method.POST, "/v1/order/cancel/all", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<CancelOrderResponse>(result.Content);
            return obj.success;
        }

        public bool SubmitWithdrawal(decimal amount, string asset, string address)
        {
            var data = new WithdrawalRequest() { amount = amount, asset = asset, address = address };
            var result = Call(Method.POST, "/v1/withdraw", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<WithdrawalResponse>(result.Content);
            return obj.success;
        }

    }
}
