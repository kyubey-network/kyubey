using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Models
{
    public class TokenIncubatorUpdateModel
    {
        public string Title { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Content { get; set; }
    }
}
