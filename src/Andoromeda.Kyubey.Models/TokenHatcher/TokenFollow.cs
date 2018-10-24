using System;
using System.Collections.Generic;
using System.Text;

namespace Andoromeda.Kyubey.Models.TokenHatcher
{
    public class TokenFollow
    {
        public Guid Id { get; set; }
        public string TokenId { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }
}
