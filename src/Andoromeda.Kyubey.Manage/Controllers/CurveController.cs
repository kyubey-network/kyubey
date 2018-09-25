using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage.Controllers
{
    public class CurveController : BaseController
    {
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await DB.Curves.ToListAsync(cancellationToken));
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Za-z0-9_]]{{4,16}}$)}/manage")]
        public async Task<IActionResult> Manage(string id, CancellationToken cancellationToken)
        {
            var curve = await DB.Curves.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (curve == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Curve not found"];
                    x.Details = SR["Curve not found"];
                });
            }
            return View(curve);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id:regex(^[[A-Za-z0-9_]]{{4,16}}$)}/manage")]
        public async Task<IActionResult> Manage(
            string id,
            string priceSupplyFunction,
            string priceBalanceFunction,
            string balanceSupplyFunction,
            string supplyBalanceFunction,
            CancellationToken cancellationToken)
        {
            var curve = await DB.Curves.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (curve == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Curve not found"];
                    x.Details = SR["Curve not found"];
                });
            }
            curve.PriceBalanceFunction = priceBalanceFunction;
            curve.PriceSupplyFunction = priceSupplyFunction;
            curve.BalanceSupplyFunction = balanceSupplyFunction;
            curve.SupplyBalanceFunction = supplyBalanceFunction;
            await DB.SaveChangesAsync();

            return Prompt(x =>
            {
                x.Title = SR["Curve updated"];
                x.Details = SR["Curve updated"];
            });
        }

        [HttpGet]
        [Route("[controller]/new")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/new")]
        public async Task<IActionResult> Create(
            string id,
            string priceSupplyFunction,
            string priceBalanceFunction,
            string balanceSupplyFunction,
            string supplyBalanceFunction,
            string arguments,
            CancellationToken cancellationToken)
        {
            var curve = new Curve
            {
                Id = id,
                Arguments = JsonConvert.SerializeObject(arguments.Split(',').Select(x => new CurveArgument { Id = x.Trim(), Name = x.Trim() }))
            };

            DB.Curves.Add(curve);
            await DB.SaveChangesAsync();
            return RedirectToAction("Manage", new { id });
        }
    }
}
