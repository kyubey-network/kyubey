using System.Collections.Generic;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class Table : Table<IDictionary<string, object>>
    {
    }

    public class Table<T>
    {
        public IEnumerable<T> rows { get; set; }
    }
}
