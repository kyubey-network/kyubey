using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenComment
    {
        public Guid Id { get; set; }
        [MaxLength(16)]
        public string TokenId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public DateTimeOffset CreateTime { get; set; }

        public long UserId { get; set; }
        public  User User { get; set; }

        public long? ReplyUserId { get; set; }
        public  User ReplyUser { get; set; }
        public bool IsDelete { get; set; }
        public DateTimeOffset? DeleteTime { get; set; }
        public string Content { get; set; }
    }
}
