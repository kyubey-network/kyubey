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
        public bool Incubation { get; set; }
        public bool Dex { get; set; }
        public bool Contract_Exchange { get; set; }
    }
    public class TokenManifestBasicJObject
    {
        public string Protocol { get; set; }
        public TokenManifestBasicContractJObject Contract { get; set; }
        public string Website { get; set; }
        public string Github { get; set; }
        public string Email { get; set; }
        public string Tg { get; set; }

    }
    public class TokenManifestBasicContractJObject
    {
        public string transfer { get; set; }
        public string pricing { get; set; }
    }
}
