using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class Currency
    {
        [MaxLength(16)]
        public string Id { get; set; }

        [MaxLength(32)]
        public string Name { get; set; }

        public byte[] Logo { get; set; }

        [MaxLength(64)]
        public string Email { get; set; }

        [MaxLength(64)]
        public string GitHub { get; set; }

        public string Description { get; set; }

        public string Alert { get; set; }

        [MaxLength(64)]
        public string Contract { get; set; }

        [MaxLength(64)]
        public string BancorTableName { get; set; }
        
        public string ScatterCode { get; set; }

        [MaxLength(64)]
        public string BalancePath { get; set; }

        [MaxLength(64)]
        public string SupplyPath { get; set; }

        [MaxLength(64)]
        public string WebUrl { get; set; }

        [MaxLength(64)]
        public string PasswordSha256 { get; set; }

        public double Price { get; set; }

        public double Change { get; set; }

        public int PRI { get; set; }

        public bool Display { get; set; }
    }
}
