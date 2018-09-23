using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andoromeda.Kyubey.Models
{
    public enum AlertLogRunResult
    {
        Success,
        Warning,
        Error
    }

    public class AlertLog
    {
        public Guid Id { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        [ForeignKey("AlertRule")]
        public long AlertRuleId { get; set; }

        public virtual AlertRule AlertRule { get; set; }

        public string Message { get; set; }
    }
}
