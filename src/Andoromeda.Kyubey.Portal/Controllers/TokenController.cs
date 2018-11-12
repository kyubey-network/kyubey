using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Models;
using Andoromeda.Kyubey.Models.Hatcher;
using Pomelo.AspNetCore.Localization;
using Andoromeda.Kyubey.Portal.Interface;
using Andoromeda.Kyubey.Portal.Services;
using Microsoft.AspNetCore.NodeServices;
using Newtonsoft.Json;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class TokenController : BaseController
    {
        public override void Prepare()
        {
            base.Prepare();

            ViewBag.Flex = true;
            ViewBag.NavActive = "KYUBEYDEX";
        }

        [HttpGet("[controller]/pair")]
        public async Task<IActionResult> Pair([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository, string token, CancellationToken cancellationToken)
        {
            if (token != null)
            {
                token = token.ToUpper();
            }
            var otcTokens = _tokenRepository.GetAll().Select(x => x.Id).ToList();

            var last = await db.MatchReceipts
                .Where(x => otcTokens.Contains(x.TokenId))
                            .Where(y => y.TokenId == token || token == null)
                            .GroupBy(x => x.TokenId)
                            .Select(x => new
                            {
                                TokenId = x.Key,
                                Last = x.LastOrDefault()
                            })
                            .ToListAsync();



            var last24 = await db.MatchReceipts
                .Where(x => otcTokens.Contains(x.TokenId))
                .Where(y => y.TokenId == token || token == null)
                .Where(y => y.Time < DateTime.UtcNow.AddDays(-1))
                .GroupBy(x => x.TokenId)
                .Select(x => new
                {
                    TokenId = x.Key,
                    Last = x.LastOrDefault()
                })
                .ToListAsync();



            var rOrdinal = last.Select(x => new
            {
                id = x.TokenId,
                price = x.Last?.UnitPrice ?? 0,
                change = (x.Last?.UnitPrice ?? 0) / (last24?.FirstOrDefault(l => l.TokenId == x.TokenId)?.Last?.UnitPrice - 1)
            });

            var r = db.Tokens.Where(x => x.Status == TokenStatus.Active).ToList().Select(x => new
            {
                id = x.Id,
                price = rOrdinal.FirstOrDefault(o => o.id == x.Id)?.price ?? 0,
                change = rOrdinal.FirstOrDefault(o => o.id == x.Id)?.change ?? 0
            });

            return Json(r);
        }

        public async Task<IActionResult> Default([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository, string id, CancellationToken cancellationToken)
        {
            var token = await db.Tokens
                .Include(x => x.Curve)
                .SingleOrDefaultAsync(x => x.Id == id
                    && x.Status == TokenStatus.Active, cancellationToken);

            if (token == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token {0} is not found", id];
                    x.StatusCode = 404;
                });
            }

            ViewBag.Curve = token.Curve;
            ViewBag.TokenInfo = _tokenRepository.GetOne(id);

            return View(await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken));
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository, string id, CancellationToken cancellationToken)
        {
            var token = await db.Tokens
                .Include(x => x.Curve)
                .SingleOrDefaultAsync(x => x.Id == id
                    && x.Status == TokenStatus.Active, cancellationToken);

            if (token == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token {0} is not found", id];
                    x.StatusCode = 404;
                });
            }

            ViewBag.Curve = token.Curve;
            ViewBag.Token = token;

            var tokenInfo = _tokenRepository.GetOne(id);
            ViewBag.TokenInfo = tokenInfo;
            ViewBag.TokenDescription = _tokenRepository.GetTokenIncubationDescription(id, currentCulture);

            if (!token.HasIncubation || tokenInfo.Incubation == null)
            {
                return View("IndexOld", token);
            }
            tokenInfo.Incubation.Begin_Time = tokenInfo.Incubation.Begin_Time ?? DateTimeOffset.MinValue;

            ViewBag.Flex = false;

            var comments = db.TokenComments
                .Include(x => x.User)
                .Include(x => x.ReplyUserId)
                .Where(x => x.TokenId == id && x.IsDelete == false)
                .Select(x => new TokenComment()
                {
                    Id = x.Id,
                    ReplyUserId = x.ReplyUserId,
                    UserId = x.UserId,
                    User = x.User,
                    Content = x.Content,
                    CreateTime = x.CreateTime,
                    DeleteTime = x.DeleteTime,
                    IsDelete = x.IsDelete,
                    ParentCommentId = x.ParentCommentId,
                    ReplyUser = db.Users.FirstOrDefault(u => u.Id == x.ReplyUserId),
                    TokenId = x.TokenId
                })
                .ToList();

            var commentPraises = await (from p in db.TokenCommentPraises
                                        join c in db.TokenComments
                                        on p.CommentId equals c.Id
                                        where c.IsDelete == false && c.TokenId == id
                                        select p).ToListAsync(cancellationToken);

            var commentsVM = comments.Where(x => x.ReplyUserId == null).OrderByDescending(x => x.CreateTime).Select(x => new TokenCommentViewModel()
            {
                Content = x.Content,
                CreateTime = x.CreateTime.ToLocalTime().ToString("d", System.Globalization.CultureInfo.InvariantCulture),
                PraiseCount = commentPraises.Count(p => p.CommentId == x.Id),
                UserId = x.UserId,
                UserName = x.User.UserName,
                ChildComments = comments.Where(cc => cc.ParentCommentId == x.Id).OrderByDescending(c => c.CreateTime).Select(c => new TokenCommentViewModel()
                {
                    Content = c.Content,
                    CreateTime = c.CreateTime.ToLocalTime().ToString("d", System.Globalization.CultureInfo.InvariantCulture),
                    PraiseCount = commentPraises.Count(p => p.CommentId == x.Id),
                    ReplyUserId = c.ReplyUserId,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    ReplyUserName = c.ReplyUser?.UserName
                }).ToList()
            }).ToList();

            var handlerVM = new TokenHandlerViewModel()
            {
                TokenInfo = await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken),
                HandlerInfo = new HandlerInfo()
                {
                    Detail = _tokenRepository.GetTokenIncubationDetail(id, currentCulture),
                    Introduction = _tokenRepository.GetTokenIncubationDescription(id, currentCulture),
                    RemainingDay = tokenInfo?.Incubation?.DeadLine == null ? -999 : Math.Max((tokenInfo.Incubation.DeadLine - DateTime.Now).Days,0),
                    TargetCredits = tokenInfo?.Incubation?.Goal ?? 0,
                    CurrentRaised = Convert.ToDecimal(await db.RaiseLogs.Where(x =>
                    (x.Timestamp > tokenInfo.Incubation.Begin_Time
                    && x.Timestamp < tokenInfo.Incubation.DeadLine)
                    && x.TokenId == token.Id && !x.Account.StartsWith("eosio.")).Select(x => x.Amount).SumAsync()),
                    CurrentRaisedCount = await db.RaiseLogs.Where(x => x.TokenId == token.Id && x.Account.Length == 12).CountAsync(),
                    BeginTime = tokenInfo?.Incubation?.Begin_Time
                },
                IncubatorBannerUrls = TokenTool.GetTokenIncubatorBannerUris(id, _tokenRepository.GetTokenIncubationBannerPaths(id, currentCulture)),
                Comments = commentsVM,
                RecentUpdate = _tokenRepository.GetTokenIncubatorUpdates(id, currentCulture)?.Select(x => new RecentUpdateViewModel()
                {
                    Content = x.Content,
                    Time = x.Time,
                    Title = x.Title
                }).ToList()
            };

            ViewBag.HandlerView = handlerVM;

            return View(token);
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/exchange")]
        public async Task<IActionResult> Exchange([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository, string id, CancellationToken cancellationToken)
        {
            return await Default(db, _tokenRepository, id, cancellationToken);
        }


        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/candlestick")]
        public async Task<IActionResult> Candlestick(string id, int period, DateTime begin, DateTime end, CancellationToken token)
        {
            var ticks = new TimeSpan(0, 0, period);
            begin = new DateTime(begin.Ticks / ticks.Ticks * ticks.Ticks);
            end = new DateTime(end.Ticks / ticks.Ticks * ticks.Ticks);

            var data = await DB.MatchReceipts
                .Where(x => x.TokenId == id)
                .Where(x => x.Time < end)
                .GroupBy(x => x.Time >= begin ? x.Time.Ticks / ticks.Ticks * ticks.Ticks : 0)
                .Select(x => new Candlestick
                {
                    Time = new DateTime(x.Key),
                    Min = x.Select(y => y.UnitPrice).Min(),
                    Max = x.Select(y => y.UnitPrice).Max(),
                    Opening = x.Select(y => y.UnitPrice).FirstOrDefault(),
                    Closing = x.Select(y => y.UnitPrice).FirstOrDefault(),
                    Volume = x.Count()
                })
                .ToListAsync();

            if (data.Count > 1)
            {
                for (var i = begin; i < end; i = i.Add(ticks))
                {
                    if (!data.Any(x => x.Time == i))
                    {
                        var prev = data
                            .Where(x => x.Time < i)
                            .OrderBy(x => x.Time)
                            .LastOrDefault();

                        if (prev != null)
                        {
                            data.Add(new Candlestick
                            {
                                Min = prev.Closing,
                                Max = prev.Closing,
                                Closing = prev.Closing,
                                Opening = prev.Closing,
                                Time = i,
                                Volume = 0
                            });
                        }
                    }
                }
            }

            return Json(data.Where(x => x.Time >= begin).OrderBy(x => x.Time));
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/publish")]
        public async Task<IActionResult> Publish([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository, string id, CancellationToken cancellationToken)
        {
            return await Default(db, _tokenRepository, id, cancellationToken);
        }


        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}.png")]
        public async Task<IActionResult> Icon([FromServices] KyubeyContext db, [FromServices] ITokenRepository _tokenRepository, string id, CancellationToken cancellationToken)
        {
            var token = await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken);
            if (token == null)
                return NotFound();
            var iconSrc = _tokenRepository.GetTokenIconPath(id);

            if (string.IsNullOrWhiteSpace(iconSrc))
            {
                return File(System.IO.File.ReadAllBytes(Path.Combine("wwwroot", "img", "null.png")), "image/png", "icon.png");
            }
            else
            {
                return File(System.IO.File.ReadAllBytes(iconSrc), "image/png", "icon.png");
            }
        }

        [HttpGet("[controller]/tokenbanner/{id}/{fileName}.png")]
        public async Task<IActionResult> Banner([FromServices] KyubeyContext db, string id, string fileName, CancellationToken cancellationToken)
        {
            var token = await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken);
            if (token == null)
                return NotFound();
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Tokens", id, "slides", fileName + ".png");
            if (!System.IO.File.Exists(filePath))
            {
                return File(System.IO.File.ReadAllBytes(Path.Combine("wwwroot", "img", "null.png")), "image/png", "icon.png");
            }
            else
            {
                return File(System.IO.File.ReadAllBytes(filePath), "image/png");
            }
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}.js")]
        public async Task<IActionResult> Javascript([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            var token = await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken);
            if (token == null)
                return NotFound();
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Tokens", id, "contract_exchange", "exchange.js");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            else
            {
                return File(System.IO.File.ReadAllBytes(filePath), "application/x-javascript");
            }
        }

        [HttpGet("[controller]/{id}/contract-price")]
        public async Task<IActionResult> ContractPrice(
          [FromServices]  INodeServices node,
             //[FromServices] KyubeyContext db,
             [FromServices] ITokenRepository _tokenRepository,
            [FromServices] IConfiguration config,
            string id,
            CancellationToken token)
        {
            try {
                using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
                {
                    var currentTokenInfo = _tokenRepository.GetOne(id);
                    var currentPriceJavascript = _tokenRepository.GetPriceJsText(id);
                    using (var tableResponse = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
                    {
                        code = currentTokenInfo.Basic.Contract.Pricing,
                        scope = currentTokenInfo.Basic.Price_Scope,
                        table = currentTokenInfo.Basic.Price_Table,
                        json = true
                    }))
                    {
                        if (string.IsNullOrWhiteSpace(currentTokenInfo.Basic.Price_Scope) || string.IsNullOrWhiteSpace(currentTokenInfo.Basic.Price_Scope))
                        {
                            return Json(new
                            {
                                BuyPrice = 0,
                                SellPrice = 0
                            });
                        }

                        var text = await tableResponse.Content.ReadAsStringAsync();
                        var rows = JsonConvert.DeserializeObject<Table>(text).rows;

                        var buy = await node.InvokeExportAsync<string>("./price", "buyPrice", rows, currentPriceJavascript);
                        var sell = await node.InvokeExportAsync<string>("./price", "sellPrice", rows, currentPriceJavascript);
                        var buyPrice = Convert.ToDouble(buy.Contains(".") ? buy.TrimEnd('0') : buy);
                        var sellPrice = Convert.ToDouble(sell.Contains(".") ? sell.TrimEnd('0') : sell);
                        return Json(new
                        {
                            BuyPrice = buyPrice,
                            SellPrice = sellPrice
                        });
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
            
        }

        [HttpGet("[controller]/{id}/buy-data")]
        public async Task<IActionResult> BuyData(
            [FromServices] KyubeyContext db,
            [FromServices] IConfiguration config,
            double? min,
            double? max,
            string id,
            CancellationToken token)
        {
            var orders = await db.DexBuyOrders
                .Where(x => x.TokenId == id)
                .OrderByDescending(x => x.UnitPrice)
                .Take(15)
                .ToListAsync();

            var totalMax = 0.0;
            if (orders.Count > 0)
            {
                totalMax = await db.DexBuyOrders
                    .Where(x => x.TokenId == id)
                    .Select(x => x.Bid)
                    .MaxAsync(token);
            }

            var ret = orders
                .Select(x => new
                {
                    unit = x.UnitPrice,
                    amount = x.Ask,
                    total = x.Bid,
                    totalMax = totalMax
                });

            return Json(ret);
        }

        [HttpGet("[controller]/{id}/sell-data")]
        public async Task<IActionResult> SellData(
            [FromServices] KyubeyContext db,
            [FromServices] IConfiguration config,
            double? min,
            double? max,
            string id,
            CancellationToken token)
        {
            var orders = await db.DexSellOrders
                .Where(x => x.TokenId == id)
                .OrderBy(x => x.UnitPrice)
                .Take(15)
                .ToListAsync();
            orders.Reverse();

            var totalMax = 0.0;
            if (orders.Count > 0)
            {
                totalMax = await db.DexSellOrders
                    .Where(x => x.TokenId == id)
                    .Select(x => x.Bid)
                    .MaxAsync(token);
            }

            var ret = orders
                .Select(x => new
                {
                    unit = x.UnitPrice,
                    amount = x.Bid,
                    total = x.Ask,
                    totalMax = totalMax
                });

            return Json(ret);
        }

        [HttpGet("[controller]/{id}/last-match")]
        public async Task<IActionResult> LastMatch(string id, CancellationToken token)
        {
            var last = await DB.MatchReceipts
                .LastOrDefaultAsync(x => x.TokenId == id, token);
            if (last == null)
            {
                return Content("0.0000");
            }
            return Content((last.UnitPrice).ToString("0.0000"));
        }

        [HttpGet("[controller]/{id}/recent-transaction")]
        public async Task<IActionResult> RecentTransaction(string id, CancellationToken token)
        {
            var ret = await DB.MatchReceipts
                .Where(x => x.TokenId == id)
                .OrderByDescending(x => x.Time)
                .Take(20)
                .ToListAsync(token);

            return Json(ret.Select(x => new
            {
                price = x.UnitPrice,
                amount = x.Ask,
                time = x.Time
            }));
        }

        [HttpGet("[controller]/{account}/current-order")]
        public async Task<IActionResult> CurrentOrder(string account, bool only = false, CancellationToken token = default)
        {
            var buy = await DB.DexBuyOrders.Where(x => x.Account == account).ToListAsync(token);
            var sell = await DB.DexSellOrders.Where(x => x.Account == account).ToListAsync(token);
            var ret = new List<CurrentOrder>(buy.Count + sell.Count);
            ret.AddRange(buy.Select(x => new CurrentOrder
            {
                id = x.Id,
                token = x.TokenId,
                type = "Buy",
                amount = x.Ask,
                price = x.UnitPrice,
                total = x.Bid,
                time = x.Time
            }));
            ret.AddRange(sell.Select(x => new CurrentOrder
            {
                id = x.Id,
                token = x.TokenId,
                type = "Sell",
                amount = x.Bid,
                price = x.UnitPrice,
                total = x.Ask,
                time = x.Time
            }));
            return Json(ret.OrderByDescending(x => x.time));
        }

        [HttpGet("[controller]/{account}/balance/{token}")]
        public async Task<IActionResult> AccountBalance([FromServices] ITokenRepository _tokenRepository, string token, string account, CancellationToken cancellationToken = default)
        {
            var t = DB.Tokens.SingleOrDefault(x => x.Id == token);
            var tokenInfo = _tokenRepository.GetOne(token);
            using (var client = new HttpClient { BaseAddress = new Uri(Configuration["TransactionNode"]) })
            using (var response = await client.PostAsJsonAsync("/v1/chain/get_table_rows", new
            {
                code = tokenInfo?.Basic?.Contract?.Transfer ?? "eosio.token",
                table = "accounts",
                scope = account,
                json = true,
                limit = 65535
            }))
            {
                var result = await response.Content.ReadAsAsync<Table>();
                var balance = result.rows.SelectMany(x => x.Values.Select(y => y.ToString())).Where(x => x.EndsWith(" " + token)).FirstOrDefault();
                if (string.IsNullOrEmpty(balance))
                {
                    return Content("0.0000");
                }
                else
                {
                    return Content(balance.Split(' ')[0]);
                }
            }
        }

        [HttpGet("[controller]/{account}/history-order")]
        public async Task<IActionResult> HistoryOrder([FromServices] ITokenRepository _tokenRepository, string id, string account, bool only = false, CancellationToken token = default)
        {
            IQueryable<MatchReceipt> matches = DB.MatchReceipts
                .Where(x => x.Bidder == account || x.Asker == account);
            if (only)
            {
                matches = matches.Where(x => x.TokenId == id);
            }

            return Json(await matches.OrderByDescending(x => x.Time).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Holders(
            [FromServices] KyubeyContext db,
            [FromServices] ITokenRepository _tokenRepository,
            string id,
            CancellationToken cancellationToken)
        {
            var token = await db.Tokens
                .Include(x => x.Curve)
                .SingleOrDefaultAsync(x => x.Id == id
                    && x.Status == TokenStatus.Active, cancellationToken);

            if (token == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token {0} is not found", id];
                    x.StatusCode = 404;
                });
            }
            var tokenInfo = _tokenRepository.GetOne(id);
            var cancel = new CancellationTokenSource();
            cancel.CancelAfter(5000);
            try
            {
                using (var client = new HttpClient() { BaseAddress = new Uri("https://cache.togetthere.cn") })
                using (var resposne = await client.GetAsync($"/token/distributed/{tokenInfo.Basic.Contract.Transfer}/{token.Id}"))
                {
                    var dic = await resposne.Content.ReadAsAsync<IDictionary<string, double>>();
                    ViewBag.Holders = dic.Select(x => new Holder
                    {
                        account = x.Key,
                        amount = x.Value.ToString("0.0000") + " " + token.Id
                    })
                    .Take(20)
                    .ToList();
                }
            }
            catch (TaskCanceledException)
            {
            }

            return View(await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken));
        }

        [HttpGet("[controller]/{account}/favorite")]
        public async Task<IActionResult> GetFavorite(string account, CancellationToken cancellationToken)
        {
            var ret = await DB.Favorites
                .Where(x => x.Account == account)
                .Select(x => x.TokenId.ToUpper())
                .ToListAsync(cancellationToken);

            return Json(ret);
        }

        [HttpPost("[controller]/{account}/favorite/{id}")]
        public async Task<IActionResult> PostFavorite(string account, string id, CancellationToken cancellationToken)
        {
            var favorite = await DB.Favorites
                .SingleOrDefaultAsync(x => x.Account == account && x.TokenId == id, cancellationToken);

            if (favorite == null)
            {
                DB.Favorites.Add(new Favorite
                {
                    Account = account,
                    TokenId = id
                });
            }
            else
            {
                DB.Favorites.Remove(favorite);
            }

            await DB.SaveChangesAsync();
            return Content("ok");
        }
    }
}
