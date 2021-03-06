﻿using System;
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
        public async Task<IActionResult> Index([FromServices] KyubeyContext db,
            [FromServices] ITokenRepository _tokenRepository)
        {
            var tokenInfoList = _tokenRepository.GetAll().Where(x => x?.Incubation != null).ToList();
            var dbIncubations = await db.Tokens.Where(x =>
                x.HasIncubation && tokenInfoList.FirstOrDefault(t => x.Id == t.Id).Incubation != null &&
                x.Status == TokenStatus.Active).ToListAsync();

            tokenInfoList.ForEach(x => x.Incubation.Begin_Time = x.Incubation.Begin_Time ?? DateTime.MinValue);

            var tokens = dbIncubations.OrderByDescending(x => x.Priority).Select(x => new TokenHandlerListViewModel()
            {
                Id = x.Id,
                BannerSrc = TokenTool.GetTokenIncubatorBannerUri(x.Id,
                    _tokenRepository.GetTokenIncubationBannerPaths(x.Id, currentCulture).FirstOrDefault()),
                CurrentRaised = x.Raised,
                Introduction = _tokenRepository.GetTokenIncubationDescription(x.Id, currentCulture),
                ShowGoExchange = true,
                TargetCredits = tokenInfoList.FirstOrDefault(s => s.Id == x.Id)?.Incubation?.Goal ?? 0,
                BeginTime = tokenInfoList.FirstOrDefault(s => s.Id == x.Id)?.Incubation?.Begin_Time
            }).ToList();

            return View(tokens);
        }

        [Route("/Dex")]
        public async Task<IActionResult> Dex([FromServices] KyubeyContext db,
            [FromServices] ITokenRepository _tokenRepository, string token = null,
            CancellationToken cancellationToken = default)
        {
            var tokenStaticInfoList = _tokenRepository.GetAll();

            if (token != null)
            {
                token = token.ToUpper();
            }

            ViewBag.SearchToken = token;
            var ret = new List<TokenDisplay>();
            var tokens = await DB.Tokens.Where(x => token == null || x.Id.Contains(token) || x.Name.Contains(token))
                .ToListAsync(cancellationToken);

            var notStartedTokens = tokenStaticInfoList
                .Where(x => (x?.Incubation?.Begin_Time).HasValue && x?.Incubation?.Begin_Time > DateTime.Now)
                .Select(x => x.Id).ToList();

            foreach (var x in tokens.Where(t=>!notStartedTokens.Contains(t.Id)))
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