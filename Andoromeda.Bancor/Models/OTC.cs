using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Bancor.Models
{
    public class OTC
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(64)]
        public string WebUrl { get; set; }

        public string Contract { get; set; }

        public byte[] Logo { get; set; }

        [MaxLength(64)]
        public string Email { get; set; }

        [MaxLength(64)]
        public string GitHub { get; set; }

        public string Description { get; set; }

        public string Alert { get; set; }

        public double PriceMin { get; set; }

        public double PriceMax { get; set; }

        public int Transaction { get; set; }
    }
}
