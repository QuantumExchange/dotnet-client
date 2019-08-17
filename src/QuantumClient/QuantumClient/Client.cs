﻿using System;
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

        public TradeOrder[] GetOpenOrders()
        {
            var result = Call(Method.GET, "/v1/orders/open", null, true);
            return JsonConvert.DeserializeObject<TradeOrder[]>(result.Content);
        }

        public TradeOrder GetOrder(string orderId)
        {
            var result = Call(Method.GET, "/v1/orders/" + orderId, null, true);
            return JsonConvert.DeserializeObject<TradeOrder>(result.Content);
        }

        public string PlaceLimitOrder(string action, decimal amount, string asset, string currency, decimal price)
        {
            var data = new PlaceOrderRequest() { action = action, amount = amount, asset = asset, currency = currency, price = price, type = "limit" };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);

            return obj.id;
        }

        public string PlaceMarketOrder(string action, decimal amount, string asset, string currency)
        {
        var data = new PlaceOrderRequest() { action = action, amount = amount, asset = asset, currency = currency, type = "market" };
            var result = Call(Method.POST, "/v1/order/new", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<PlaceOrderResponse>(result.Content);
            return obj.id;
        }

        public bool CancelOrder(string orderId, string major, string minor)
        {
            var data = new CanceOrderRequest() { id = orderId, major = major, minor = minor };
            var result = Call(Method.POST, "/v1/order/cancel", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<CancelOrderResponse>(result.Content);

            return obj.success;
        }

        public bool CancelAllOrders(string major, string minor)
        {
            var data = new CancelAllOrdersRequest() { major = major, minor = minor };
            var result = Call(Method.POST, "/v1/order/cancel/all", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<CancelOrderResponse>(result.Content);
            return obj.success;
        }

        public bool SubmitWithdrawal(decimal amount, string asset) {
            var data = new WithdrawalRequest() { amount = amount, asset = asset };
            var result = Call(Method.POST, "/v1/withdraw", JsonConvert.SerializeObject(data), true);
            var obj = JsonConvert.DeserializeObject<WithdrawalResponse>(result.Content);
            return obj.success;
        }

    }
}
