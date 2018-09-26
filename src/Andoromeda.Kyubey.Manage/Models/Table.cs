using System.Collections.Generic;

namespace Andoromeda.Kyubey.Manage.Models
{
    public class Table
    {
        public IEnumerable<IDictionary<string, object>> rows { get; set; }
    }
}
