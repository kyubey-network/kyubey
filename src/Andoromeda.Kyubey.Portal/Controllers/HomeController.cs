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
using Pomelo.AspNetCore.Localization;
using Andoromeda.Kyubey.Portal.Interface;
using Andoromeda.Kyubey.Portal.Services;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class HomeController : BaseController
    {
        [Route("/")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository)
        {
            var tokenInfoList = _tokenRepository.GetAll().ToList();
            var dbIncubations = await db.Tokens.Where(x => x.HasIncubation && x.Status == TokenStatus.Active).ToListAsync();
            var tokens = dbIncubations.OrderByDescending(x => x.Priority).Select(x => new TokenHandlerListViewModel()
            {
                Id = x.Id,
                BannerSrc = TokenTool.GetTokenIncubatorBannerUri(x.Id, _tokenRepository.GetTokenIncubationBannerPaths(x.Id, currentCulture).FirstOrDefault()),
                CurrentRaised = x.Raised,
                Introduction = _tokenRepository.GetTokenIncubationDescription(x.Id, currentCulture),
                ShowGoExchange = true,
                TargetCredits = tokenInfoList.FirstOrDefault(s => s.Id == x.Id)?.Incubation?.RaisedTarget ?? 0
            }).ToList();

            return View(tokens);
        }

        [Route("/Dex")]
        public async Task<IActionResult> Dex([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository, string token = null, CancellationToken cancellationToken = default)
        {
            var tokenStaticInfoList = _tokenRepository.GetAll();

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
                .Join(tokenStaticInfoList, b => b.Id, t => t.Id, (b, t) => new
                {
                    Bancor = b,
                    TokenInfo = t
                })
                .OrderByDescending(x => x.TokenInfo.Priority).Select(x => x.Bancor)
                .ToList();

            IEnumerable<Otc> otcs = db.Otcs.Include(x => x.Token);
            if (!string.IsNullOrEmpty(token))
            {
                otcs = otcs.Where(x => x.Id.Contains(token) || x.Token.Name.Contains(token));
            }
            otcs = otcs
                .Where(x => x.Status == Status.Active)
                .Join(tokenStaticInfoList, o => o.Id, t => t.Id, (o, t) => new
                {
                    Otc = o,
                    TokenInfo = t
                })
                .OrderByDescending(x => x.TokenInfo.Priority).Select(x => x.Otc)
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
