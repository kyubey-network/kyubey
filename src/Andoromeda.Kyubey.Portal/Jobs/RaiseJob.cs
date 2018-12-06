using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Interface;
using Microsoft.Extensions.Configuration;
using Pomelo.AspNetCore.TimedJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Jobs
{
    public class RaiseJob : Job
    {
        //[Invoke(Begin = "2018-06-01", Interval = 1000 * 30, SkipWhileExecuting = true)]
        public void PollDexActions(IConfiguration config, KyubeyContext db, ITokenRepository tokenRepository)
        {
            foreach(var x in db.Tokens
                .Where(x => x.HasIncubation && x.HasContractExchange)
                .ToList())
            {
                var token = tokenRepository.GetOne(x.Id);

                if (token.Incubation.Begin_Time.HasValue && DateTime.UtcNow < TimeZoneInfo.ConvertTimeToUtc(token.Incubation.Begin_Time.Value))
                {
                    continue;
                }

                if (DateTime.UtcNow > TimeZoneInfo.ConvertTimeToUtc(token.Incubation.DeadLine))
                {
                    continue;
                }

                var depot = token.Basic.Contract.Depot ?? token.Basic.Contract.Transfer;
                var balance = GetBalanceAsync(depot, config).Result;
                x.Raised = Convert.ToDecimal(balance);
                db.SaveChanges();
            }
        }

        private async Task<double> GetBalanceAsync(string account, IConfiguration config)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var response = await client.PostAsJsonAsync("/v1/chain/get_currency_balance", new
            {
                code = "eosio.token",
                account = account,
                symbol = "EOS"
            }))
            {
                var txt = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadAsAsync<IEnumerable<string>>();
                var balance = result.FirstOrDefault();
                if (string.IsNullOrEmpty(balance))
                {
                    return 0;
                }
                else
                {
                    return Convert.ToDouble(balance.Split(' ')[0]);
                }
            }
        }
    }
}
