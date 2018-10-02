using System;
using System.Collections.Generic;
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
            using (var client = new HttpClient() { BaseAddress = new Uri("https://cache.togetthere.cn") })
            using (var response = await client.GetAsync($"/token/distributed/dacincubator/KBY", token))
            {
                var result = await response.Content.ReadAsAsync<IDictionary<string, double>>(token);
                var ret = result.Select(x => new Holder
                    {
                        Account = x.Key,
                        Asset = x.Value.ToString("0.0000") + " KBY"
                    })
                    .Take(10);
                return View(ret);
            }
        }
    }
}
