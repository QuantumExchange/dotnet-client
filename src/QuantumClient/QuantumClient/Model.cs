using System;

namespace Quantum
{
    public class OrderBookItem{
        public string id;
        public decimal amount;
        public decimal filled;
        public decimal price;
    }

    public class OpenOrderItem
    {
        public string id;
        public string action;
        public decimal amount;
        public decimal filled;
        public decimal price;
    }

    public class StopLossOrderItem
    {
        public string id;
        public string action;
        public decimal amount;
        public decimal filled;
        public decimal price;
        public decimal stop_price;
        public string type;
    }
    
    public class GetOrderBookResponse
    {
        public string asset;
        public string currency;
        public OrderBookItem[] asks;
        public OrderBookItem[] bids;
        public DateTime timestamp;
    }

    public class Balance {
        public string id;
        public  decimal balance;
    }

    public class PlaceOrderRequest
    {
        public string action;
        public decimal amount;
        public decimal quote_amount;
        public string asset;
        public string currency;
        public decimal price;
        public decimal stop_price;
        public string type;
        public string[] options;
    }

    public class PlaceOrderResponse
    {
        public string id;
    }

    public class CanceOrderRequest
    {
        public string id;
        public string asset;
        public string currency;

    }

    public class CancelAllOrdersRequest
    {
        public string asset;
        public string currency;

    }

    public class CancelOrderResponse
    {
        public bool success;

    }

    public class WithdrawalRequest
    {
        public decimal amount;
        public string asset;
        public string address;
    }

    public class WithdrawalResponse
    {
        public bool success;
    }

}