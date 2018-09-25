using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Andoromeda.Kyubey.Models
{
    public enum CurveArgumentControlType
    {
        Input,
        Slider
    }

    public class CurveArgument
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public double? Min { get; set; }

        public double? Max { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CurveArgumentControlType ControlType { get; set; }
    }

    public class Curve
    {
        [MaxLength(16)]
        public string Id { get; set; }

        public string Description { get; set; }

        [MaxLength(128)]
        public string PriceSupplyFunction { get; set; }

        [MaxLength(128)]
        public string PriceBalanceFunction { get; set; }

        [MaxLength(128)]
        public string BalanceSupplyFunction { get; set; }

        [MaxLength(128)]
        public string SupplyBalanceFunction { get; set; }

        public JsonObject<IEnumerable<CurveArgument>> Arguments { get; set; } = "[]";
    }
}
