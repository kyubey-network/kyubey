using System;
using System.Collections.Generic;

namespace Andoromeda.Kyubey.Portal.Models
{
    public abstract class Order
    {
        public int id { get; set; }

        public long timestamp { get; set; }

        public long unit_price { get; set; }

        public abstract bool IsBidValid(string symbol, string contract = null);

        public abstract bool IsAskValid(string symbol, string contract = null);

        public abstract double GetUnitPrice();
    }

    public class OtcOrder : Order
    {
        public string owner { get; set; }

        public Asset bid { get; set; }

        public Asset ask { get; set; }

        public override bool IsAskValid(string symbol, string contract)
        {
            return ask.quantity.EndsWith(" " + symbol) && ask.contract == contract;
        }

        public override bool IsBidValid(string symbol, string contract)
        {
            return bid.quantity.EndsWith(" " + symbol) && bid.contract == contract;
        }

        public override double GetUnitPrice()
        {
            if (bid.quantity.EndsWith(" EOS"))
            {
                return Convert.ToDouble(bid.quantity.Split(' ')[0]) / Convert.ToDouble(ask.quantity.Split(' ')[0]);
            }
            else
            {
                return Convert.ToDouble(ask.quantity.Split(' ')[0]) / Convert.ToDouble(bid.quantity.Split(' ')[0]);
            }
        }
    }

    public class DexOrder : Order
    {
        public string account { get; set; }

        public string bid { get; set; }

        public string ask { get; set; }

        public override bool IsAskValid(string symbol, string contract = null)
        {
            return ask.EndsWith(" " + symbol);
        }

        public override bool IsBidValid(string symbol, string contract)
        {
            return bid.EndsWith(" " + symbol);
        }

        public override double GetUnitPrice()
        {
            if (bid.EndsWith(" EOS"))
            {
                return Convert.ToDouble(bid.Split(' ')[0]) / Convert.ToDouble(ask.Split(' ')[0]);
            }
            else
            {
                return Convert.ToDouble(ask.Split(' ')[0]) / Convert.ToDouble(bid.Split(' ')[0]);
            }
        }
    }

    public class OrderTable<T>
    {
        public IEnumerable<T> rows { get; set; }
    }

    public class UserOrder
    {
        public long orderid { get; set; }

        public string symbol { get; set; }
    }
}
