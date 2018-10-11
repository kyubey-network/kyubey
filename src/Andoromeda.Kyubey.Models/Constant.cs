using System.ComponentModel.DataAnnotations;

namespace Andoromeda.Kyubey.Models
{
    public class Constant
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(64)]
        public string Value { get; set; }
    }
}
