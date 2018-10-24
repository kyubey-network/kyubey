using System;
using System.Collections.Generic;
using System.Text;

namespace Andoromeda.Kyubey.Models.TokenHatcher
{
    public class TokenProvider
    {
        public Guid Id { get; set; }
        public int RoleType { get; set; }
        public string UserId { get; set; }
    }
}
