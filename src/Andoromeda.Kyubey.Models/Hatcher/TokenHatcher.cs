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
        //public Guid Id { get; set; }
        //[ForeignKey("Token")]
        [MaxLength(16)]
        public string TokenId { get; set; }
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
    }
}
