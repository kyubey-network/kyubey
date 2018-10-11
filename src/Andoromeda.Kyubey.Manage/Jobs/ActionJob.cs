using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Pomelo.AspNetCore.TimedJob;
using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Manage.Models;

namespace Andoromeda.Kyubey.Manage.Jobs
{
    public class ActionJob : Job
    {
        [Invoke(Begin = "2018-06-01", Interval = 1000 * 60 * 1, SkipWhileExecuting = true)]
        public void PollActions(IConfiguration config, KyubeyContext db)
        {
            TryHandleActionAsync(config, db).Wait();
        }

        private async Task<int> TryHandleActionAsync(IConfiguration config, KyubeyContext db)
        {
            var cnt = 0;
            while(true)
            {
                var act = await LookupActionAsync(config, db);
                if (act == null)
                {
                    return cnt;
                }
                if (act.action_trace.act.name != "matchreceipt")
                {
                    continue;
                }

                var data = act.action_trace.act.data.t;
                var bid = Convert.ToDouble(data.bid.Split(' ')[0]);
                var ask = Convert.ToDouble(data.ask.Split(' ')[0]);
                var unit_price = (double)bid / (double)ask;
                var tokenId = data.ask.Split(' ')[1];
                
                db.MatchReceipts.Add(new MatchReceipt
                {
                    Id = act.global_action_seq.ToString(),
                    Ask = ask,
                    Bid = bid,
                    Asker = data.asker,
                    Bidder = data.bidder,
                    Time = act.block_time,
                    TokenId = tokenId,
                    UnitPrice = unit_price
                });

                await db.SaveChangesAsync();
                ++cnt;
            }
        }

        private async Task<EosAction> LookupActionAsync(IConfiguration config, KyubeyContext db)
        {
            var row = await db.Constants.SingleAsync(x => x.Id == "action_pos");
            var position = Convert.ToInt64(row.Value);
            ++position;
            using (var client = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var response = await client.PostAsJsonAsync("/v1/history/get_actions", new
            {
                account_name = "kyubeydex.bp",
	            pos = position,
	            offset= 0
            }))
            {
                var txt = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadAsAsync<EosActionWrap>();
                if (result.actions.Count() == 0)
                {
                    return null;
                }

                row.Value = position.ToString();
                await db.SaveChangesAsync();

                return result.actions.First();
            }
        }
    }
}
