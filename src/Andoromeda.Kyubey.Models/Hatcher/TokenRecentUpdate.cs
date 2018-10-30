using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenRecentUpdate
    {
        public Guid Id { get; set; }
        [MaxLength(16)]
        public string TokenId { get; set; }
        public DateTimeOffset OperateTime { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; }
        //public int OperateType { get; set; }
        public string Content { get; set; }
    }
}
