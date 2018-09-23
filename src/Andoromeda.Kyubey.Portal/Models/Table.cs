using System.Collections.Generic;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class Table
    {
        public IEnumerable<IDictionary<string, object>> rows { get; set; }
    }
}
