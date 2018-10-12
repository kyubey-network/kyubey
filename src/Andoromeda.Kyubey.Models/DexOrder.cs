using System;
using System.ComponentModel.DataAnnotations;

namespace Andoromeda.Kyubey.Models
{
    public class DexOrder
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(16)]
        public string Account { get; set; }

        [MaxLength(16)]
        public string TokenId { get; set; }

        public double Ask { get; set; }

        public double Bid { get; set; }

        public double UnitPrice { get; set; }

        public DateTime Time { get; set; }
    }
}
