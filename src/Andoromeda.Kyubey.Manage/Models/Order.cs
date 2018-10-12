using System.Collections.Generic;

namespace Andoromeda.Kyubey.Manage.Models
{
    public class Order
    {
        public int id { get; set; }

        public string account { get; set; }

        public string bid { get; set; }

        public string ask { get; set; }

        public long unit_price { get; set; }

        public long timestamp { get; set; }
    }

    public class OrderTable
    {
        public IEnumerable<Order> rows { get; set; }
    }
}
