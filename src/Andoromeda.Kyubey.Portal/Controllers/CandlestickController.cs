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
        [HttpPost("{id}")]
        public async Task<ActionResult> Post([FromServices] KyubeyContext db, string id, [FromBody] CandlestickItem item)
        {
            var maxPeriod = 60;
            TimeSpan groupTimeSpan = TimeSpan.FromMinutes(Math.Min(item.period, maxPeriod));

            if (item.perioidUnit == PerioidUnit.hour)
            {
                groupTimeSpan = TimeSpan.FromHours(Math.Min(item.period, maxPeriod));
            }
            else if (item.perioidUnit == PerioidUnit.day)
            {
                groupTimeSpan = TimeSpan.FromDays(Math.Min(item.period, maxPeriod));
            }
            //will to mem cache or redis
            var dbQueryList = (from s in db.MatchReceipts
                               where s.TokenId == id && s.Time >= UnixTimeStampToDateTime(item.begin) && s.Time < UnixTimeStampToDateTime(item.end)
                               orderby s.Time
                               select s
                               ).ToList();

            var grouped = (from s in dbQueryList
                           group s by
                                 new
                                 {
                                     groupedColumn = (int)((s.Time - DateTime.MinValue).TotalMinutes / groupTimeSpan.TotalMinutes)
                                 }
                            into g
                           select new
                           {
                               TimeStamp = DateTime.MinValue.AddMinutes(g.Key.groupedColumn * groupTimeSpan.TotalMinutes).ToTimeStamp(),
                               Min = g.Min(x => x.UnitPrice),
                               Max = g.Max(x => x.UnitPrice),
                               First = g.First().UnitPrice,
                               Last = g.Last().UnitPrice
                           }).ToList();
            return Json(grouped);
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        [HttpGet("api/[controller]")]
        public ActionResult Get()
        {
            return Content("True");
        }
        public class CandlestickItem
        {
            public int period { get; set; }
            public PerioidUnit perioidUnit { get; set; }
            public long begin { get; set; }
            public long end { get; set; }
        }
        public enum PerioidUnit
        {
            minute,
            hour,
            day
        }
    }
}
