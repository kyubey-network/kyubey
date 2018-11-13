using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andoromeda.Kyubey.Models
{
    public enum TokenAlertPlan
    {
        Free,
        Basic,
        Standard,
        Premium
    }
    public enum TokenAlertNotificationType
    {
        None,
        CallApi,
        SpecifiedPerson,
        CallThePersonFromApi
    }

    public enum TokenStatus
    {
        Active,
        DisabledByOwner,
        DisabledByAdmin
    }

    public class Token
    {
        [MaxLength(16)]
        public string Id { get; set; }

        [MaxLength(32)]
        public string Name { get; set; }

        [MaxLength(16)]
        [ForeignKey("Curve")]
        public string CurveId { get; set; }

        public virtual Curve Curve { get; set; }

        [MaxLength(255)]
        public string Alert { get; set; }

        public int Priority { get; set; }
        
        public TokenAlertPlan AlertPlan { get; set; }

        public TokenAlertNotificationType NotificationType { get; set; }

        public TokenStatus Status { get; set; }

        public bool HasIncubation { get; set; }

        public bool HasDex { get; set; }

        public bool HasContractExchange { get; set; }

        public decimal Raised { get; set; }

        public long ActionPosition { get; set; }

        public virtual ICollection<RaiseLog> RaiseLogs { get; set; }
    }
}
