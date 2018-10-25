using System;
using System.Collections.Generic;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenComment
    {
        public Guid Id { get; set; }
        public Guid? ParentCommentId { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string UserId { get; set; }
        public string ReplyUserId { get; set; }
        public bool IsDelete { get; set; }
        public DateTimeOffset? DeleteTime { get; set; }
        public string Content { get; set; }
    }
}
