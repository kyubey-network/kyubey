using System;
using System.ComponentModel.DataAnnotations;

namespace Andoromeda.Kyubey.Models
{
    public class RaiseLog
    {
        public Guid Id { get; set; }

        [MaxLength(16)]
        public string TokenId { get; set; }

        [MaxLength(16)]
        public string Account { get; set; }

        public double Amount { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
