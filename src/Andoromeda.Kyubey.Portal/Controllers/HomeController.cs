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
                CurrentRaised = Convert.ToDecimal(db.RaiseLogs.Where(y => y.TokenId == x.Id && y.Account.Length == 12).Select(y => y.Amount).Sum()),
                Introduction = _tokenRepository.GetTokenIncubationDescription(x.Id, currentCulture),
                ShowGoExchange = true,
                TargetCredits = tokenInfoList.FirstOrDefault(s => s.Id == x.Id)?.Incubation?.Goal ?? 0
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
            var ret = new List<TokenDisplay>();
            var tokens = await DB.Tokens.ToListAsync(cancellationToken);
            foreach (var x in tokens)
            {
                if (!x.HasDex && !x.HasContractExchange)
                {
                    continue;
                }

                var t = new TokenDisplay
                {
                    Id = x.Id,
                    Name = x.Name,
                    Protocol = x.CurveId ?? SR["Unknown"],
                    ExchangeInDex = x.HasDex,
                    ExchangeInContract = x.HasContractExchange
                };

                ret.Add(t);
            }

            return View(ret);
        }
    }
}
