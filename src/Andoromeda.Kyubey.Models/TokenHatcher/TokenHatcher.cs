using System;
using System.Collections.Generic;
using System.Text;

namespace Andoromeda.Kyubey.Models.TokenHatcher
{
    public class TokenHatcher
    {
        public string TokenId { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public DateTimeOffset Deadline { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public decimal TargetCredits { get; set; }
        public string Detail { get; set; }
    }
}
