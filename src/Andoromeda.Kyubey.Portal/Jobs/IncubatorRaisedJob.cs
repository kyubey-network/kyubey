using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Interface;
using Andoromeda.Kyubey.Portal.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pomelo.AspNetCore.TimedJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Jobs
{
    public class IncubatorRaisedJob : Job
    {
        //[Invoke(Begin = "2018-06-01", Interval = 1000 * 60 * 1, SkipWhileExecuting = true)]
        public void SyncAllIncubatorRaised(IConfiguration config, KyubeyContext db, ITokenRepository tokenRepository)
        {
            var hatchingTokens = tokenRepository.GetAll().Where(x => x.Incubation != null).ToList();
            using (var client = new HttpClient() { BaseAddress = new Uri(config["TransactionNode"]) })
            {
                hatchingTokens.ForEach(x => SyncOneIncubatorRaised(client, x, db).Wait());
            }

        }
        private async Task SyncOneIncubatorRaised(HttpClient client, TokenManifestJObject token, KyubeyContext db)
        {
            if (!string.IsNullOrWhiteSpace(token?.Basic?.Contract?.Transfer))
            {
                using (var response = await client.PostAsJsonAsync("/v1/chain/get_table_rows", new
                {
                    code = "eosio.token",
                    scope = token.Basic.Contract.Transfer,
                    table = "accounts",
                    json = true
                }))
                {
                    var txt = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<IncubatorRaisedResponse>(txt, new JsonSerializerSettings
                    {
                        Error = HandleDeserializationError
                    });
                    try
                    {
                        var raisedVal = result.Rows.FirstOrDefault().Balance.Split(' ')[0];
                        var dbToken = db.Tokens.FirstOrDefault(x => x.Id == token.Id);
                        dbToken.Raised = Convert.ToDecimal(raisedVal);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }
        private void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }

        private class IncubatorRaisedResponse
        {
            public IList<IncubatorRaisedResponseRow> Rows { get; set; }

        }
        private class IncubatorRaisedResponseRow
        {
            public string Balance { get; set; }
        }
    }
}
