using System;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class Candlestick
    {
        public double Opening { get; set; }

        public double Closing { get; set; }

        public double Max { get; set; }

        public double Min { get; set; }

        public DateTime Time { get; set; }
        public int Volume { get; set; }
    }
}
