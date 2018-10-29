using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class TokenHandlerListVM
    {
        public string BannerId { get; set; }
        public string Id { get; set; }
        public string Introduction { get; set; }
        public bool ShowGoExchange { get; set; }
        public decimal CurrentRaised { get; set; }
        public decimal TargetCredits { get; set; }
    }
}
