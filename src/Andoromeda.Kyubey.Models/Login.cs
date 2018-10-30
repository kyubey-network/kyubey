using System;
using System.ComponentModel.DataAnnotations;

namespace Andoromeda.Kyubey.Models
{
    public class Login
    {
        [MaxLength(64)]
        public string Id { get; set; }

        [MaxLength(16)]
        public string Account { get; set; }

        public DateTime Time { get; set; } = DateTime.UtcNow;
    }
}
