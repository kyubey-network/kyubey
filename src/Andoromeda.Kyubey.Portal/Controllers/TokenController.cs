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
        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            var token = await db.Tokens
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
            ViewBag.Dex = await db.Dexes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            return View(await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken));
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/sell")]
        public async Task<IActionResult> Sell([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            return await Index(db, id, cancellationToken);
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/buy")]
        public async Task<IActionResult> Buy([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            return await Index(db, id, cancellationToken);
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/publish")]
        public async Task<IActionResult> Publish([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            return await Index(db, id, cancellationToken);
        }

        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/kyubey/candlestick")]
        public async Task<IActionResult> KyubeyCandlestick([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
        {
            return await Index(db, id, cancellationToken);
        }
        
        [HttpGet("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/curve")]
        public async Task<IActionResult> Curve([FromServices] KyubeyContext db, string id, CancellationToken cancellationToken)
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
            ViewBag.Dex = await db.Dexes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            ViewBag.Curve = token.Curve;

            return View(await db.Tokens.SingleAsync(x => x.Id == id && x.Status == TokenStatus.Active, cancellationToken));
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

        [HttpGet("[controller]/{id}/buy-data")]
        public async Task<IActionResult> BuyData(
            [FromServices] KyubeyContext db,
            [FromServices] IConfiguration config,
            double? min,
            double? max,
            string id,
            CancellationToken token)
        {
            var otc = await db.Otcs.Include(x => x.Token).SingleAsync(x => x.Id == id, token);
            using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var tableResponse1 = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
            {
                code = "eosotcbackup",
                scope = "eosio.token",
                table = "order",
                json = true,
                limit = 65535
            }))
            {
                IEnumerable<Order> rows = JsonConvert.DeserializeObject<OrderTable>((await tableResponse1.Content.ReadAsStringAsync()))
                    .rows
                    .Where(y => y.bid.quantity.EndsWith(otc.Id) && y.bid.contract == otc.Token.Contract)
                    .Where(y => y.ask.quantity.EndsWith("EOS") && y.ask.contract == "eosio.token");
                if (min.HasValue)
                {
                    rows = rows.Where(x => Convert.ToDouble(x.ask.quantity.Split(' ')[0]) / Convert.ToDouble(x.bid.quantity.Split(' ')[0]) >= min.Value);
                }
                if (max.HasValue)
                {
                    rows = rows.Where(x => Convert.ToDouble(x.ask.quantity.Split(' ')[0]) / Convert.ToDouble(x.bid.quantity.Split(' ')[0]) <= max.Value);
                }
                return Json(rows.OrderBy(x => Convert.ToDouble(x.ask.quantity.Split(' ')[0]) / Convert.ToDouble(x.bid.quantity.Split(' ')[0])));
            }
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
            var otc = await db.Otcs.Include(x => x.Token).SingleAsync(x => x.Id == id, token);
            using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var tableResponse1 = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
            {
                code = "eosotcbackup",
                scope = otc.Token.Contract,
                table = "order",
                json = true,
                limit = 65535
            }))
            {
                IEnumerable<Order> rows = JsonConvert.DeserializeObject<OrderTable>((await tableResponse1.Content.ReadAsStringAsync()))
                    .rows
                    .Where(y => y.ask.quantity.EndsWith(otc.Id) && y.ask.contract == otc.Token.Contract)
                    .Where(y => y.bid.quantity.EndsWith("EOS") && y.bid.contract == "eosio.token");
                if (min.HasValue)
                {
                    rows = rows.Where(x => Convert.ToDouble(x.bid.quantity.Split(' ')[0]) / Convert.ToDouble(x.ask.quantity.Split(' ')[0]) >= min.Value);
                }
                if (max.HasValue)
                {
                    rows = rows.Where(x => Convert.ToDouble(x.bid.quantity.Split(' ')[0]) / Convert.ToDouble(x.ask.quantity.Split(' ')[0]) <= max.Value);
                }
                return Json(rows.OrderBy(x => Convert.ToDouble(x.bid.quantity.Split(' ')[0]) / Convert.ToDouble(x.ask.quantity.Split(' ')[0])));
            }
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
            ViewBag.Dex = await db.Dexes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

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
