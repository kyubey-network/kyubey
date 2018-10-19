using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andoromeda.Kyubey.Models
{
    public class Favorite
    {
        [MaxLength(16)]
        public string Account { get; set; }

        [MaxLength(16)]
        [ForeignKey("Token")]
        public string TokenId { get; set; }

        public virtual Token Token { get; set; }
    }
}
