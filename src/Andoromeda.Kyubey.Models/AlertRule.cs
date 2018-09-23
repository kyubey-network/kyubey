using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andoromeda.Kyubey.Models
{
    public enum AlertRuleInterval
    {
        Min5,
        Min10,
        Min30,
        Hour1,
        Hour2,
        Hour6,
        Hour12,
        Day1
    }

    public enum AlertRuleType
    {
        Action,
        Table,
        Balance,
        CpuMemoryAndBandwidth,
        TouchWeb
    }

    public enum AlertRuleHeathState
    {
        Healthy,
        Warning,
        Error
    }

    public class AlertRule
    {
        public long Id { get; set; }

        [MaxLength(16)]
        [ForeignKey("Token")]
        public string TokenId { get; set; }

        public virtual Token Token { get; set; }

        public int Shard { get; set; }

        public AlertRuleType Type { get; set; }

        public AlertRuleInterval Interval { get; set; }

        public AlertRuleHeathState HealthState { get; set; }

        public string RunnerJavascript { get; set; }

        public virtual ICollection<AlertLog> AlertLogs { get; set; }
    }
}
