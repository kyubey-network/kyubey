using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenCrowdfunding
    {
        public Guid Id { get; set; }
        [MaxLength(16)]
        public string TokenId { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public decimal CrowdfundingCount { get; set; }

    }
}
