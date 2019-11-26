﻿using System;

namespace Quantum
{
    public class TradeOrder{
        public string id;
        public string action;
        public decimal amount;
        public decimal filled;
        public string asset;
        public string currency;
        public decimal price;
        public string status;
        public DateTime created_on;
        public DateTime updated_on;
        public Int64 sequence;

    }

    public class GetOrderBookResponse
    {
        public string asset;
        public string currency;
        public TradeOrder[] asks;
        public TradeOrder[] bids;
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
        public string asset;
        public string currency;
        public decimal price;
        public string type;
    }

    public class PlaceOrderResponse
    {
        public string id;

    }

    public class CanceOrderRequest
    {
        public string id;
        public string major;
        public string minor;

    }

    public class CancelAllOrdersRequest
    {
        public string major;
        public string minor;

    }

    public class CancelOrderResponse
    {
        public bool success;

    }

    public class WithdrawalRequest
    {
        public decimal amount;
        public string asset;
    }

    public class WithdrawalResponse
    {
        public bool success;
    }

}