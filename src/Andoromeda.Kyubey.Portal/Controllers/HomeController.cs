using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class HomeController : BaseController
    {
        [Route("/")]
        [Route("/kyubey")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, CancellationToken cancellationToken)
        {
            var tokens = await db.Bancors
                .Include(x => x.Token)
                .Where(x => x.Status == Status.Active)
                .OrderByDescending(x => x.Token.Priority)
                .ToListAsync(cancellationToken);
            return View(tokens);
        }

        [Route("/otc")]
        public async Task<IActionResult> OTC([FromServices] KyubeyContext db, CancellationToken cancellationToken)
        {
            var tokens = await db.Otcs
                .Include(x => x.Token)
                .Where(x => x.Status == Status.Active)
                .OrderByDescending(x => x.Token.Priority)
                .ToListAsync(cancellationToken);
            return View(tokens);
        }

        [Route("/dex")]
        public async Task<IActionResult> DEX([FromServices] KyubeyContext db, CancellationToken cancellationToken)
        {
            var tokens = await db.Dexes
                .Include(x => x.Token)
                .Where(x => x.Status == Status.Active)
                .OrderByDescending(x => x.Token.Priority)
                .ToListAsync(cancellationToken);
            return View(tokens);
        }
    }
}
