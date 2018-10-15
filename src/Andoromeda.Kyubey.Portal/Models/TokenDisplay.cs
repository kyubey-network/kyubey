namespace Andoromeda.Kyubey.Portal.Models
{
    public class TokenDisplay
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool ExchangeInContract { get; set; }

        public bool ExchangeInDex { get; set; }

        public double Price { get; set; }

        public double Change { get; set; }

        public string Protocol { get; set; }
    }
}
