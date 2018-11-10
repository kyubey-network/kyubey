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
        public async Task<ActionResult> Post([FromServices] KyubeyContext db, string id, [FromBody] CandlestickRequest item)
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
            //will storage to memcache or redis
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
                           select new CandlestickResponse
                           {
                               Time = DateTime.MinValue.AddMinutes(g.Key.groupedColumn * groupTimeSpan.TotalMinutes),
                               TimeStamp = DateTime.MinValue.AddMinutes(g.Key.groupedColumn * groupTimeSpan.TotalMinutes).ToTimeStamp(),
                               Min = g.Min(x => x.UnitPrice),
                               Max = g.Max(x => x.UnitPrice),
                               First = g.First().UnitPrice,
                               Last = g.Last().UnitPrice,
                               Count = g.Count()
                           }).ToList();
            var r = TransCandlestickResponses(UnixTimeStampToDateTime(item.begin), UnixTimeStampToDateTime(item.end), groupTimeSpan, grouped);
            return Json(r);
        }
        /// <summary>
        ///  repair data
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="interval"></param>
        /// <param name="ordinalData"></param>
        /// <returns></returns>
        private List<CandlestickResponse> TransCandlestickResponses(DateTime start, DateTime end, TimeSpan interval, List<CandlestickResponse> ordinalData)
        {
            if (ordinalData.Count == 0)
                return ordinalData;
            var response = new List<CandlestickResponse>();
            for (var current = start; current < end; current = current.Add(interval))
            {
                var target = ordinalData.FirstOrDefault(x => x.Time >= current && x.Time < current.Add(interval));
                if (target != null)
                {
                    response.Add(target);
                }
                else
                {
                    response.Add(new CandlestickResponse()
                    {
                        Time = current,
                        TimeStamp = current.ToTimeStamp(),
                        First = 0.0004,
                        Last = 0.0003,
                        Max = 0.001,
                        Min = 0.0002
                    });
                }
            }
            return response;
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
        public class CandlestickRequest
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
        public class CandlestickResponse
        {
            public DateTime Time { get; set; }
            public long TimeStamp { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public double First { get; set; }
            public double Last { get; set; }
            public double Count { get; set; }
        }
        
    }
}
