using System.Collections.Generic;

namespace Andoromeda.Kyubey.Manage.Models
{
    public class Order
    {
        public int id { get; set; }

        public string owner { get; set; }

        public Asset bid { get; set; }

        public Asset ask { get; set; }

        public long timestamp { get; set; }
    }

    public class OrderTable
    {
        public IEnumerable<Order> rows { get; set; }
    }
}
