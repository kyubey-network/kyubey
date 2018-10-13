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
using Newtonsoft.Json;
using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Models;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class TokenController : BaseController
    {
        public override void Prepare()
        {
            base.Prepare();

            ViewBag.Flex = true;
        }

        [HttpGet("[controller]/pair")]
        public async Task<IActionResult> Pair([FromServices] KyubeyContext db, string token, CancellationToken cancellationToken)
        {
            IQueryable<Otc> ret = db.Otcs
                .Where(x => x.Status == Status.Active);

            if (!string.IsNullOrEmpty(token))
            {
                ret = ret.Where(x => x.Id.Contains(token));
            }

            return Json(await ret.Select(x => new
            {
                id = x.Id,
                price = x.Price,
                change = x.Change
            }).ToListAsync(cancellationToken));
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
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

            ViewBag.Otc = await db.Otcs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            ViewBag.Bancor = await db.Bancors.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            ViewBag.Curve = token.Curve;

            return View(await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken));
        }
        
        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/exchange")]
        public async Task<IActionResult> Exchange([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            return await Index(db, id, cancellationToken);
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/publish")]
        public async Task<IActionResult> Publish([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            return await Index(db, id, cancellationToken);
        }
        
        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}.png")]
        public async Task<IActionResult> Icon([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            var token = await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken);
            if (token.Icon == null || token.Icon.Length == 0)
            {
                return File(System.IO.File.ReadAllBytes(Path.Combine("wwwroot", "img", "null.png")), "image/png", "icon.png");
            }
            else
            {
                return File(token.Icon, "image/png", "icon.png");
            }
        }
        
        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}.js")]
        public async Task<IActionResult> Javascript([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            var token = await db.Bancors
                .SingleAsync(x => x.Id == id, cancellationToken);
            return Content(token.TradeJavascript, "application/x-javascript");
        }

        [HttpGet("[controller]/{id}/contract-price")]
        public async Task<IActionResult> ContractPrice(
            [FromServices] KyubeyContext db,
            [FromServices] IConfiguration config,
            string id,
            CancellationToken token)
        {
            var bancor = await db.Bancors.SingleOrDefaultAsync(x => x.Id == id);
            return Json(new
            {
                BuyPrice = bancor.BuyPrice,
                SellPrice = bancor.SellPrice
            });
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

            var ret = orders
                .Select(x => new
                {
                    unit = x.UnitPrice,
                    amount = x.Ask,
                    total = x.Bid
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

            var ret = orders
                .Select(x => new
                {
                    unit = x.UnitPrice,
                    amount = x.Ask,
                    total = x.Bid
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
            return Content(last.UnitPrice.ToString());
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
        public async Task<IActionResult> CurrentOrder(string id, string account, bool only = false, CancellationToken token = default)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(Configuration["TransactionNode"]) })
            using (var response = await client.PostAsJsonAsync("/v1/chain/get_table_rows", new
            {
                code = Configuration["Contract:Otc"],
                scope = account,
                table = "userorder",
                json = true,
                limit = 65535
            }))
            {
                var table = await response.Content.ReadAsAsync<OrderTable<UserOrder>>();
                var tasks = table.rows.Select(x => GetOrder(client, x.orderid, x.symbol, token));
                await Task.WhenAll(tasks);
                var ret = new List<CurrentOrder>(tasks.Count());
                foreach(var x in tasks)
                {
                    ret.Add(await x);
                }
                return Json(ret);
            }
        }

        private async Task<CurrentOrder> GetOrder(HttpClient client, long id, string token, CancellationToken cancellationToken = default)
        {
            using (var response = await client.PostAsJsonAsync("/v1/chain/get_table_rows", new
            {
                code = Configuration["Contract:Otc"],
                scope = token,
                table = id > 0 ? "buyorder" : "sellorder",
                json = true,
                index_position = 1,
                lower_bound = Math.Abs(id),
                limit = 1
            }))
            {
                var table = await response.Content.ReadAsAsync<OrderTable<Models.DexOrder>>();
                var order = table.rows.FirstOrDefault();
                if (id > 0)
                {
                    return new CurrentOrder
                    {
                        id = Math.Abs(id),
                        token = token,
                        amount = Convert.ToDouble(order.ask.Split(' ')[0]),
                        price = order.unit_price / 10000.0,
                        time = new DateTime(1970, 1, 1).AddSeconds(order.timestamp),
                        type = "Buy"
                    };

                }
                else
                {
                    return new CurrentOrder
                    {
                        id = Math.Abs(id),
                        token = token,
                        amount = Convert.ToDouble(order.bid.Split(' ')[0]),
                        price = order.unit_price / 10000.0,
                        time = new DateTime(1970, 1, 1).AddSeconds(order.timestamp),
                        type = "Sell"
                    };

                }
            }
        }

        [HttpGet("[controller]/{account}/history-order")]
        public async Task<IActionResult> HistoryOrder(string id, string account, bool only = false, CancellationToken token = default)
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
            string id, 
            CancellationToken cancellationToken)
        {
            var token = await db.Tokens
                .Include(x => x.Curve)
                .SingleOrDefaultAsync(x => x.Id == id
                    && x.Status == TokenStatus.Active, cancellationToken);

            ViewBag.Otc = await db.Otcs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            ViewBag.Bancor = await db.Bancors.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (token == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token {0} is not found", id];
                    x.StatusCode = 404;
                });
            }

            var cancel = new CancellationTokenSource();
            cancel.CancelAfter(5000);
            try
            {
                using (var client = new HttpClient() { BaseAddress = new Uri("https://cache.togetthere.cn") })
                using (var resposne = await client.GetAsync($"/token/distributed/{token.Contract}/{token.Id}"))
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
    }
}
