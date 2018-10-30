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
using Andoromeda.Kyubey.Portal.Models;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class HomeController : BaseController
    {
        [Route("/")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db)
        {
            //linq 待优化
            var tokens = await db.TokenHatchers
                .Include(x => x.Token)
                .Select(x => new TokenHandlerListVM()
                {
                    BannerId = db.TokenBanners.Where(b => b.TokenId == x.TokenId).OrderBy(b => b.BannerOrder).FirstOrDefault().Id.ToString(),
                    Id = x.TokenId,
                    Introduction = x.Introduction,
                    TargetCredits = x.TargetCredits,
                    CurrentRaised = x.CurrentRaisedSum,
                    ShowGoExchange = true,
                })
                .ToListAsync();

            return View(tokens);
        }

        [Route("/Dex")]
        public async Task<IActionResult> Dex([FromServices] KyubeyContext db, string token = null, CancellationToken cancellationToken = default)
        {
            if (token != null)
            {
                token = token.ToUpper();
            }
            ViewBag.SearchToken = token;
            IEnumerable<Bancor> bancors = db.Bancors.Include(x => x.Token);
            if (!string.IsNullOrEmpty(token))
            {
                bancors = bancors.Where(x => x.Id.Contains(token) || x.Token.Name.Contains(token));
            }
            bancors = bancors
                .Where(x => x.Status == Status.Active)
                .OrderByDescending(x => x.Token.Priority)
                .ToList();

            IEnumerable<Otc> otcs = db.Otcs.Include(x => x.Token);
            if (!string.IsNullOrEmpty(token))
            {
                otcs = otcs.Where(x => x.Id.Contains(token) || x.Token.Name.Contains(token));
            }
            otcs = otcs
                .Where(x => x.Status == Status.Active)
                .OrderByDescending(x => x.Token.Priority)
                .ToList();

            var ret = new List<TokenDisplay>();
            var tokens = await DB.Tokens.ToListAsync(cancellationToken);
            foreach (var x in tokens)
            {
                if (!bancors.Any(y => y.Id == x.Id) && !otcs.Any(y => y.Id == x.Id))
                {
                    continue;
                }

                var t = new TokenDisplay
                {
                    Id = x.Id,
                    Name = x.Name,
                    Protocol = x.CurveId ?? SR["Unknown"],
                    ExchangeInDex = otcs.Any(y => y.Id == x.Id),
                    ExchangeInContract = bancors.Any(y => y.Id == x.Id)
                };

                if (t.ExchangeInDex)
                {
                    t.Change = otcs.Single(y => y.Id == x.Id).Change;
                    t.Price = otcs.Single(y => y.Id == x.Id).Price;
                }
                else
                {
                    t.Change = bancors.Single(y => y.Id == x.Id).Change;
                    t.Price = bancors.Single(y => y.Id == x.Id).BuyPrice;
                }

                ret.Add(t);
            }

            return View(ret);
        }
    }
}
