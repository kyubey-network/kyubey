using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenHatcher
    {
        [Key]
        [MaxLength(16)]
        [ForeignKey("Token")]
        public string TokenId { get; set; }
        [MaxLength(64)]
        public string Title { get; set; }
        public virtual Token Token { get; set; }

        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; }
        public int CurrentRaisedCount { get; set; }
        public decimal CurrentRaisedSum { get; set; }
        //public virtual Token Token { get; set; }

        public DateTimeOffset StartTime { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public DateTimeOffset Deadline { get; set; }
        /// <summary>
        /// ???
        /// </summary>
        public decimal TargetCredits { get; set; }
        public string Detail { get; set; }
        public string Introduction { get; set; }
    }
}
