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

        public byte[] Icon { get; set; }

        [MaxLength(32)]
        public string Name { get; set; }

        public string Description { get; set; }

        public long? DeliveryAmount { get; set; }


        //public string Detail { get; set; }

        [MaxLength(16)]
        [ForeignKey("Curve")]
        public string CurveId { get; set; }

        public virtual Curve Curve { get; set; }

        public JsonObject<IEnumerable<decimal>> CurveArguments { get; set; } = "[]";

        [MaxLength(64)]
        public string WebUrl { get; set; }

        [MaxLength(64)]
        public string GitHub { get; set; }

        [MaxLength(64)]
        public string Email { get; set; }

        [MaxLength(64)]
        public string Contract { get; set; }

        [MaxLength(255)]
        public string Alert { get; set; }

        public int Priority { get; set; }

        [ForeignKey("User")]
        public long UserId { get; set; }

        public virtual User User { get; set; }

        public TokenAlertPlan AlertPlan { get; set; }

        public TokenAlertNotificationType NotificationType { get; set; }

        public TokenStatus Status { get; set; }
    }
}
