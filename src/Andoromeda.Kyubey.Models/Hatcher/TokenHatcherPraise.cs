using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public class TokenHatcherPraise
    {
        public Guid Id { get; set; }
        [MaxLength(16)]
        public string TokenId { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }
}
