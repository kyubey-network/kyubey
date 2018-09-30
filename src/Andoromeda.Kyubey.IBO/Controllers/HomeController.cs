using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Andoromeda.Kyubey.IBO.Models;

namespace Andoromeda.Kyubey.IBO.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/buy")]
        public IActionResult Buy([FromServices] IConfiguration config)
        {
            ViewBag.Status = config["Status"];
            ViewBag.Time = config["Time"];
            return View();
        }

        [Route("/sell")]
        public IActionResult Sell([FromServices] IConfiguration config)
        {
            ViewBag.Exchange = Convert.ToDouble(config["Exchange"]).ToString("0.0000");
            return View();
        }

        [Route("/help")]
        public IActionResult Help()
        {
            return View();
        }

        [Route("/orders")]
        public async Task<IActionResult> Orders([FromServices] IConfiguration config, CancellationToken token)
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var response = await client.PostAsJsonAsync("/v1/history/get_actions", new
            {
                account_name = "dacincubator"
            }, token))
            {
                var actions = await response.Content.ReadAsAsync<GetActionsRespose>(token);
                var ret = actions.actions.Where(x => x.action_trace.act.account == "eosio.token"
                    && x.action_trace.act.name == "transfer").Select(x => new Claim
                    {
                        Account = x.action_trace.act.data.to,
                        Asset = x.action_trace.act.data.quantity
                    });
                return View(ret);
            }
        }
    }
}
