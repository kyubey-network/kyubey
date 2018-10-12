using System;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class CurrentOrder
    {
        public long id { get; set; }

        public string type { get; set; }

        public double price { get; set; }

        public double amount { get; set; }

        public string token { get; set; }

        public DateTime time { get; set; }
    }
}
