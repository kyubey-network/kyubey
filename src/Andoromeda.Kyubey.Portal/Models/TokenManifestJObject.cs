using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class TokenManifestJObject
    {
        public string Id { get; set; }
        public string[] Owners { get; set; }
        public int Priority { get; set; }
        public TokenManifestBasicJObject Basic { get; set; }
        public IncubationJObject Incubation { get; set; }
        public bool Dex { get; set; }
        public bool Contract_Exchange { get; set; }
    }
    public class IncubationJObject
    {
        public decimal Goal { get; set; }
        public DateTimeOffset DeadLine { get; set; }
        public DateTimeOffset? Begin_Time { get; set; }
    }
    public class TokenManifestBasicJObject
    {
        public string Protocol { get; set; }
        public TokenManifestBasicContractJObject Contract { get; set; }
        public string Website { get; set; }
        public string Github { get; set; }
        public string Email { get; set; }
        public string Tg { get; set; }
        public decimal? Total_Supply { get; set; }
        public decimal[] Curve_Arguments { get; set; }
        public string Price_Table { get; set; }
        public string Price_Scope { get; set; }
    }
    public class TokenManifestBasicContractJObject
    {
        public string Transfer { get; set; }
        public string Pricing { get; set; }
    }
}
