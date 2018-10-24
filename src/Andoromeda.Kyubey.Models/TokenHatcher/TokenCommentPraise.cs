using System;
using System.Collections.Generic;
using System.Text;

namespace Andoromeda.Kyubey.Models.TokenHatcher
{
    public class TokenCommentPraise
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public bool IsPraise { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }
}
