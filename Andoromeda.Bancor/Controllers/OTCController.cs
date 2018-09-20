using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Andoromeda.Bancor.Models;

namespace Andoromeda.Bancor.Controllers
{
    public class OTCController : Controller
    {
        [HttpGet("[controller]/{id}")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.OTCs.SingleAsync(x => x.Id == id, token));
        }

        [HttpGet("[controller]/{id}/buy")]
        public async Task<IActionResult> Buy([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.OTCs.SingleAsync(x => x.Id == id, token));
        }

        [HttpGet("[controller]/{id}/sell")]
        public async Task<IActionResult> Sell([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.OTCs.SingleAsync(x => x.Id == id, token));
        }

        [HttpGet("[controller]/{id}/publish")]
        public async Task<IActionResult> Publish([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.OTCs.SingleAsync(x => x.Id == id, token));
        }
        
        [HttpGet("[controller]/{id}/icon")]
        public async Task<IActionResult> Icon([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            var bytes = (await db.OTCs.SingleAsync(x => x.Id == id, token)).Logo;
            if (bytes == null || bytes.Length == 0)
            {
                bytes = System.IO.File.ReadAllBytes("wwwroot/img/null.png");
            }
            return File(bytes, "image/png");
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
            var otc = await db.OTCs.SingleAsync(x => x.Id == id, token);
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
                    .Where(y => y.bid.quantity.EndsWith(otc.Id) && y.bid.contract == otc.Contract)
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
            var otc = await db.OTCs.SingleAsync(x => x.Id == id, token);
            using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var tableResponse1 = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
            {
                code = "eosotcbackup",
                scope = otc.Contract,
                table = "order",
                json = true,
                limit = 65535
            }))
            {
                IEnumerable<Order> rows = JsonConvert.DeserializeObject<OrderTable>((await tableResponse1.Content.ReadAsStringAsync()))
                    .rows
                    .Where(y => y.ask.quantity.EndsWith(otc.Id) && y.ask.contract == otc.Contract)
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
    }
}
