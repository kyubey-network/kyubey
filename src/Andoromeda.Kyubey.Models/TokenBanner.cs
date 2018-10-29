
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andoromeda.Kyubey.Models
{
    public class TokenBanner
    {
        public Guid Id { get; set; }
        [MaxLength(16)]
        public string TokenId { get; set; }
        public byte[] Banner { get; set; }
        public int BannerOrder { get; set; }
    }
}
