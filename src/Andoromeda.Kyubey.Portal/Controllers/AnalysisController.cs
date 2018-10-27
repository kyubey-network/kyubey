using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Andoromeda.Kyubey.Portal.Controllers
{
    public class AnalysisController : BaseController
    {
        public override void Prepare()
        {
            base.Prepare();

            ViewBag.NavActive = "ANALYSIS";
        }
        [HttpGet]
        [Route("[controller]/{id?}")]
        public async Task<IActionResult> Index(string id, CancellationToken cancellationToken)
        {
            var curves = await DB.Curves.ToListAsync(cancellationToken);
            if (curves.Count == 0)
            {
                return Prompt(x =>
                {
                    x.Title = SR["No available curve"];
                    x.Details = SR["No available curve"];
                    x.StatusCode = 404;
                });
            }
            ViewBag.CurveId = id ?? curves.First().Id;
            return View(curves);
        }
    }
}
