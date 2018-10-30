using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Andoromeda.Kyubey.Models.Hatcher
{
    public enum TokenProviderRole
    {
        Sponsor,
        Member
    }
    public class TokenProvider
    {
        public Guid Id { get; set; }
        [MaxLength(16)]
        public string TokenId { get; set; }
        public TokenProviderRole RoleType { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; }
    }
}
