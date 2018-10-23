using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pomelo.AspNetCore.TimedJob;
using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Models;

namespace Andoromeda.Kyubey.Portal.Jobs
{
    public class ActionJob : Job
    {
        [Invoke(Begin = "2018-06-01", Interval = 1000 * 5, SkipWhileExecuting = true)]
        public void PollActions(IConfiguration config, KyubeyContext db)
        {
            TryHandleActionAsync(config, db).Wait();
        }
        private async Task TryHandleActionAsync(IConfiguration config, KyubeyContext db)
        {
            while (true)
            {
                var actions = await LookupActionAsync(config, db);
                foreach (var act in actions)
                {
                    Console.WriteLine($"Handling action log {act.account_action_seq} {act.action_trace.act.name}");
                    var blockTime = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(act.block_time + 'Z'));
                    switch (act.action_trace.act.name)
                    {
                        case "Login":
                            await HandleLoginAsync(db, act.action_trace.act.data, blockTime);
                            break;
                        default:
                            continue;
                    }
                }

                if (actions.Count() < 20)
                {
                    break;
                }
            }
        }

        private async Task HandleLoginAsync(KyubeyContext db, ActionDataWrap data, DateTime time)
        {
          
        }

        private async Task<IEnumerable<EosAction>> LookupActionAsync(IConfiguration config, KyubeyContext db)
        {
            var row = await db.Constants.SingleAsync(x => x.Id == "action_pos");
            var position = Convert.ToInt64(row.Value);
            using (var client = new HttpClient { BaseAddress = new Uri(config["TransactionNodeBackup"]) })
            using (var response = await client.PostAsJsonAsync("/v1/history/get_actions", new
            {
                account_name = "kyubeydex.bp",
                pos = position,
                offset = 100
            }))
            {
                var txt = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<EosActionWrap>(txt, new JsonSerializerSettings
                {
                    Error = HandleDeserializationError
                });
                if (result.actions.Count() == 0)
                {
                    return null;
                }
                if (result.actions.Count() > 0)
                {
                    row.Value = result.actions.Last().account_action_seq.ToString();
                    await db.SaveChangesAsync();
                }

                return result.actions;
            }

        }
        private void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }
    }
}