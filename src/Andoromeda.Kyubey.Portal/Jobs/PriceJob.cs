using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Interface;
using Andoromeda.Kyubey.Portal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pomelo.AspNetCore.TimedJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Jobs
{
    public class PriceJob : Job
    {
        [Invoke(Begin = "2018-06-01", Interval = 1000 * 60 * 1, SkipWhileExecuting = true)]
        public void CalculateBancor(IConfiguration config, KyubeyContext db, INodeServices node, ITokenRepository tokenRepository)
        {
            CalculateBancorAsync(config, db, node, tokenRepository).Wait();
        }

        [Invoke(Begin = "2018-06-01", Interval = 1000 * 60 * 1, SkipWhileExecuting = true)]
        public void CalculateOTC(IConfiguration config, KyubeyContext db)
        {
            CalculateOTCAsync(config, db).Wait();
        }

        private async Task CalculateBancorAsync(IConfiguration config, KyubeyContext db, INodeServices node, ITokenRepository tokenRepository)
        {
            var tokenInfoList = tokenRepository.GetAll();
            var tokenHasContractList = tokenInfoList.Select(x => new
            {
                Id = x.Id,
                HasContract = !string.IsNullOrWhiteSpace(x?.Basic?.Contract?.Transfer)
            }).ToList();
            var tokens = db.Bancors
                .Include(x => x.Token)
                .ToList()
                .Where(x => tokenHasContractList.FirstOrDefault(t => t.Id == x.Id).HasContract);


            var time = DateTime.UtcNow;
            using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var kClient = new HttpClient { BaseAddress = new Uri(config["Kdata"]) })
            {
                foreach (var x in tokens)
                {
                    try
                    {
                        var currentTokenInfo = tokenInfoList.FirstOrDefault(t => t.Id == x.Id);
                        var currentPriceJavascript = tokenRepository.GetPriceJsText(x.Id);
                        using (var tableResponse = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
                        {
                            code = currentTokenInfo.Basic.Contract.Transfer,
                            scope = currentTokenInfo.Basic.Price_Scope,
                            table = currentTokenInfo.Basic.Price_Table,
                            json = true
                        }))
                        using (var k24hResponse = await kClient.GetAsync($"/api/Candlestick/kyubey-{x.Id}/24h"))
                        {
                            var text = await tableResponse.Content.ReadAsStringAsync();
                            var text2 = await k24hResponse.Content.ReadAsStringAsync();
                            var rows = JsonConvert.DeserializeObject<Table>(text).rows;
                            var k24h = 0.0;
                            try
                            {
                                k24h = JsonConvert.DeserializeObject<K24HResponse>(text2).data;
                            }
                            catch { }

                            var buy = await node.InvokeExportAsync<string>("./price", "buyPrice", rows, currentPriceJavascript);
                            var sell = await node.InvokeExportAsync<string>("./price", "sellPrice", rows, currentPriceJavascript);
                            x.BuyPrice = Convert.ToDouble(buy.Contains(".") ? buy.TrimEnd('0') : buy);
                            x.SellPrice = Convert.ToDouble(sell.Contains(".") ? sell.TrimEnd('0') : sell);

                            if (k24h != 0.0)
                            {
                                x.Change = x.BuyPrice / k24h - 1.0;
                            }
                            else
                            {
                                x.Change = 0.0;
                            }

                            Console.WriteLine($"Uploading {x.Id} data...");
                            using (await kClient.PostAsJsonAsync($"/api/Candlestick", new
                            {
                                values = new[] {
                                    new
                                    {
                                        catalog = "kyubey-" + x.Id,
                                        price = x.BuyPrice,
                                        utcTime = time
                                    }
                                }
                            })) { }
                            await db.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                { }
            }
            await db.SaveChangesAsync();
        }

        private async Task CalculateOTCAsync(IConfiguration config, KyubeyContext db)
        {
            using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNodeBackup"]) })
            {
                foreach (var x in await db.Otcs.ToListAsync())
                {
                    try
                    {
                        var last = await db.MatchReceipts
                            .Where(y => y.TokenId == x.Id)
                            .LastOrDefaultAsync();

                        var last24 = await db.MatchReceipts
                            .Where(y => y.TokenId == x.Id)
                            .Where(y => y.Time < DateTime.UtcNow.AddDays(-1))
                            .LastOrDefaultAsync();

                        if (last == null)
                        {
                            continue;
                        }

                        var price = last.UnitPrice;
                        var price24 = last24?.UnitPrice;

                        try
                        {
                            using (var kClient = new HttpClient { BaseAddress = new Uri(config["Kdata"]) })
                            {
                                Console.WriteLine($"Uploading {x.Id} data...");
                                using (await kClient.PostAsJsonAsync($"/api/Candlestick", new
                                {
                                    values = new[] {
                                    new
                                    {
                                        catalog = "kyubey-dex-" + x.Id,
                                        price = price,
                                        utcTime = DateTime.UtcNow
                                    }
                                }
                                })) { }
                            }
                        }
                        catch { }

                        x.Price = price;
                        x.Change = price / (price24 ?? 1.0) - 1.0;
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private double ObjectToDouble(object obj)
        {
            if (obj.ToString().Contains(" "))
            {
                return Convert.ToDouble(obj.ToString().Split(' ')[0]);
            }
            else
            {
                return Convert.ToDouble(obj);
            }
        }

        private IDictionary<string, object> ConvertObjectToDictionary(object src)
        {
            var json = JsonConvert.SerializeObject(src);
            return JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
        }
    }
}
