using System;
using System.Collections.Generic;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenRecentUpdate
    {
        public Guid Id { get; set; }
        public string TokenId { get; set; }

        public DateTimeOffset OperateTime { get; set; }
        public string OperaterUserId { get; set; }
        public int OperateType { get; set; }
        public string Content { get; set; }
    }
}
