using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenCommentPraise
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public bool IsPraise { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }
}
