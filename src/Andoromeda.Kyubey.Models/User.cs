using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Andoromeda.Kyubey.Models
{
    public class User : IdentityUser<long>
    {
        public virtual ICollection<Token> Tokens { get; set; }
    }
}
