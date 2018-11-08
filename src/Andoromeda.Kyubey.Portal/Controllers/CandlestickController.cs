using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Andoromeda.Kyubey.Portal.Controllers
{
    [Route("api/[controller]")]
    public class CandlestickController : Controller
    {
        //[HttpPost("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        //public async Task<ActionResult> Post([FromServices] KyubeyContext db, string id, int period, PerioidUnit perioidUnit, DateTime begin, DateTime end)
        //{
        //    var maxPeriod = 60;
        //    TimeSpan groupTimeSpan = TimeSpan.FromMinutes(Math.Min(period, maxPeriod));

        //    if (perioidUnit == PerioidUnit.hour)
        //    {
        //        groupTimeSpan = TimeSpan.FromHours(Math.Min(period, maxPeriod));
        //    }
        //    else if (perioidUnit == PerioidUnit.day)
        //    {
        //        groupTimeSpan = TimeSpan.FromDays(Math.Min(period, maxPeriod));
        //    }

        //    var d = DateTime.Now - DateTime.Now;
        //    var grouped = from s in db.MatchReceipts
        //                  where s.TokenId == id
        //                  group s by
        //                     s.Time.Ticks / groupTimeSpan.Ticks
        //                   into g
        //                  select new
        //                  {
        //                      g.Key,
        //                      Min =
        //                  }


        //}
        public enum PerioidUnit
        {
            minute,
            hour,
            day
        }
    }
}
