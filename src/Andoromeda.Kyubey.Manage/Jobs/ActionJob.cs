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

        [Invoke(Begin = "2018-06-01", Interval = 1000 * 60 * 1, SkipWhileExecuting = true)]
        public void PollOrders(IConfiguration config, KyubeyContext db)
        {
            PollOrdersAsync(config, db).Wait();
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

        private async Task PollOrdersAsync(IConfiguration config, KyubeyContext db)
        {
            var tokens = await db.Otcs
                .Where(x => x.Status == Status.Active)
                .ToListAsync();

            foreach(var x in tokens)
            {
                try
                {
                    Console.WriteLine($"Polling {x.Id} sell orders...");
                    await PollOrdersOfTokenAsync(config, db, x.Id, true);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                try
                {
                    Console.WriteLine($"Polling {x.Id} buy orders...");
                    await PollOrdersOfTokenAsync(config, db, x.Id, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private async Task PollOrdersOfTokenAsync(IConfiguration config, KyubeyContext db, string tokenId, bool isSell = false)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(config["TransactionNodeBackup"]) })
            using (var response = await client.PostAsJsonAsync("/v1/chain/get_table_rows", new
            {
                code = config["DexContract"],
                table = isSell ? "sellorder" : "buyorder",
                limit = 65535,
                scope = tokenId,
                json = true
            }))
            {
                var txt = await response.Content.ReadAsStringAsync();
                var table = await response.Content.ReadAsAsync<OrderTable>();
                if (isSell)
                {
                    db.DexSellOrders.RemoveRange(db.DexSellOrders.Where(x => x.TokenId == tokenId));
                    foreach (var x in table.rows)
                    {
                        db.DexSellOrders.Add(new DexSellOrder
                        {
                            Id = x.id.ToString(),
                            Account = x.account,
                            Ask = Convert.ToDouble(x.ask.Split(' ')[0]),
                            Bid = Convert.ToDouble(x.bid.Split(' ')[0]),
                            Time = new DateTime(1970, 1, 1).AddSeconds(x.timestamp),
                            TokenId = tokenId,
                            UnitPrice = x.unit_price
                        });
                    }
                    await db.SaveChangesAsync();
                }
                else
                {
                    db.DexSellOrders.RemoveRange(db.DexSellOrders.Where(x => x.TokenId == tokenId));
                    foreach (var x in table.rows)
                    {
                        db.DexSellOrders.Add(new DexSellOrder
                        {
                            Account = x.account,
                            Ask = Convert.ToDouble(x.ask.Split(' ')[0]),
                            Bid = Convert.ToDouble(x.bid.Split(' ')[0]),
                            Time = new DateTime(1970, 1, 1).AddSeconds(x.timestamp),
                            TokenId = tokenId,
                            UnitPrice = x.unit_price
                        });
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
