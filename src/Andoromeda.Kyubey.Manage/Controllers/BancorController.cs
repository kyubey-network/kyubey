using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage.Controllers
{
    public class BancorController : BaseController
    {
        [Route("[controller]/all")]
        public IActionResult Index(string name)
        {
            IEnumerable<Bancor> ret = DB.Bancors.Include(x => x.Token);

            if (!string.IsNullOrWhiteSpace(name))
            {
                ret.Where(x => x.Token.Name.Contains(name) || x.Id.Contains(name));
            }

            // Only display owned tokens if the current user is not in root role.
            if (!User.IsInRole("Root"))
            {
                ret = ret.Where(x => x.Token.UserId == User.Current.Id);
            }

            return PagedView(ret);
        }
    }
}
