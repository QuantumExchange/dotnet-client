using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Quantum
{
    public class Client
    {

        string serverUrl = "https://api.quantum.exchange";
        string key;
        string secret;

        HttpClient client = new HttpClient();

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

        private string Call(string method, string endPoint, string data, bool priv)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri(serverUrl + endPoint);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (priv)
            {
                requestMessage.Headers.Add("QUANTUM_API_KEY", key);
                var nonce = GetNonce();
                var signature = HMAC(secret, nonce.ToString() + method + endPoint + data);
                requestMessage.Headers.Add("QUANTUM_API_SIGNATURE", signature);
                requestMessage.Headers.Add("QUANTUM_API_NONCE", nonce.ToString());
            };

            HttpResponseMessage response;

            if (method.Equals("POST"))
            {
                if (!string.IsNullOrEmpty(data))
                {
                    requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");

                }
            }

            response = client.SendAsync(requestMessage).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            throw new Exception(response.Content.ReadAsStringAsync().Result);

        }

        public GetOrderBookResponse GetOrderBook(string asset, string currency)
        {
            var result = Call("GET", "/v1/order_book/" + asset + "/" + currency, null, false);
            return JsonConvert.DeserializeObject<GetOrderBookResponse>(result);
        }

        public Balance[] GetBalance()
        {
            var result = Call("GET", "/v1/balance", null, true);
            return JsonConvert.DeserializeObject<Balance[]>(result);
        }

        public OpenOrderItem[] GetOpenOrders(string asset, string currency)
        {
            var result = Call("GET", "/v1/orders/" + asset + "/" + currency + "/open", null, true);
            return JsonConvert.DeserializeObject<OpenOrderItem[]>(result);
        }

        public StopLossOrderItem[] GetTakeProfitOrders(string asset, string currency)
        {
            var result = Call("GET", "/v1/orders/" + asset + "/" + currency + "/take_profit", null, true);
            return JsonConvert.DeserializeObject<StopLossOrderItem[]>(result);
        }

        public StopLossOrderItem[] GetStopLossOrders(string asset, string currency)
        {
            var result = Call("GET", "/v1/orders/" + asset + "/" + currency + "/stop_loss", null, true);
            return JsonConvert.DeserializeObject<StopLossOrderItem[]>(result);
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
            var result = Call("POST", "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result);

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
            var result = Call("POST", "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result);
            return obj.id;
        }

        public string PlaceQuotedMarketOrder(string action, decimal quote_amount, string asset, string currency)
        {
            var data = new PlaceOrderRequest()
            {
                action = action,
                quote_amount = quote_amount,
                asset = asset,
                currency = currency,
                type = "market"
            };
            var result = Call("POST", "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result);
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
            var result = Call("POST", "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result);

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
                action = action,
                amount = amount,
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
            var result = Call("POST", "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result);

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
                ,
                options = options
            };
            var result = Call("POST", "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result);

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
            var result = Call("POST", "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result);

            return obj.id;
        }

        public bool CancelOrder(string orderId, string asset, string currency)
        {
            var data = new CanceOrderRequest() { id = orderId, asset = asset, currency = currency };
            var result = Call("POST", "/v1/order/cancel", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<CancelOrderResponse>(result);

            return obj.success;
        }

        public bool CancelAllOrders(string asset, string currency)
        {
            var data = new CancelAllOrdersRequest() { asset = asset, currency = currency };
            var result = Call("POST", "/v1/order/cancel/all", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<CancelOrderResponse>(result);
            return obj.success;
        }

        public bool SubmitWithdrawal(decimal amount, string asset, string address)
        {
            var data = new WithdrawalRequest() { amount = amount, asset = asset, address = address };
            var result = Call("POST", "/v1/withdraw", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<WithdrawalResponse>(result);
            return obj.success;
        }

    }
}
