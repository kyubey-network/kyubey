using System;
namespace Andoromeda.Kyubey.Portal.Extensions
{
    public static class CommonExtensions
    {
        public static string ToTokenPriceString(this double price)
        {
            return price.ToString("0.########");
        }
        public static string ToTokenPriceString(this decimal price)
        {
            return price.ToString("0.########");
        }
    }
}
