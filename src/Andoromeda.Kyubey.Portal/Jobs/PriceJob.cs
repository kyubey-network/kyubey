using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.AspNetCore.TimedJob;
using Andoromeda.Kyubey.Portal.Models;

namespace Andoromeda.Kyubey.Portal.Jobs
{
    public class PriceJob : Job
    {
        [Invoke(Begin = "2018-06-01", Interval = 1000 * 60 * 10, SkipWhileExecuting = true)]
        public void CalculateBancor(IConfiguration config, KyubeyContext db)
        {
            CalculateBancorAsync(config, db).Wait();
        }

        [Invoke(Begin = "2018-06-01", Interval = 1000 * 60 * 10, SkipWhileExecuting = true)]
        public void CalculateOTC(IConfiguration config, KyubeyContext db)
        {
            CalculateOTCAsync(config, db).Wait();
        }

        private async Task CalculateBancorAsync(IConfiguration config, KyubeyContext db)
        {
            var currencies = db.Currencies.ToList();
            var upload = new List<object>();
            var time = DateTime.UtcNow;
            using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            using (var kClient = new HttpClient { BaseAddress = new Uri(config["Kdata"]) })
            {
                foreach (var x in currencies)
                {
                    try
                    {
                        using (var tableResponse = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
                        {
                            code = x.Contract,
                            scope = x.Contract,
                            table = x.BancorTableName,
                            json = true
                        }))
                        using (var k24hResponse = await kClient.GetAsync($"/api/Candlestick/kyubey-{x.Id}/24h"))
                        {
                            var text = await tableResponse.Content.ReadAsStringAsync();
                            var text2 = await k24hResponse.Content.ReadAsStringAsync();
                            var row = JsonConvert.DeserializeObject<Table>(text).rows.First();
                            var k24h = 0.0;
                            try
                            {
                                k24h = JsonConvert.DeserializeObject<K24HResponse>(text2).data;
                            }
                            catch { }
                            double balance, supply;

                            if (x.BalancePath.Contains("."))
                            {
                                var split = x.BalancePath.Split('.');
                                balance = ObjectToDouble((ConvertObjectToDictionary(row[split[0]]))[split[1]]);
                            }
                            else
                            {
                                balance = ObjectToDouble(row[x.BalancePath]);
                            }

                            if (x.SupplyPath.Contains("."))
                            {
                                var split = x.SupplyPath.Split('.');
                                supply = ObjectToDouble((ConvertObjectToDictionary(row[split[0]]))[split[1]]);
                            }
                            else
                            {
                                supply = ObjectToDouble(row[x.SupplyPath]);
                            }
                            
                            x.Price = balance / supply;
                            if (k24h != 0.0)
                            {
                                x.Change = (x.Price - k24h) / k24h;
                            }
                            else
                            {
                                x.Change = 0.0;
                            }
                            upload.Add(new
                            {
                                catalog = "kyubey-" + x.Id,
                                price = x.Price,
                                utcTime = time
                            });
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                Console.WriteLine($"Uploading {upload.Count} data...");
                using (await kClient.PostAsJsonAsync($"/api/Candlestick", new
                {
                    values = upload
                }))
                { }
            }
            await db.SaveChangesAsync();
        }

        private async Task CalculateOTCAsync(IConfiguration config, KyubeyContext db)
        {
            using (var txClient = new HttpClient { BaseAddress = new Uri(config["TransactionNode"]) })
            {
                foreach (var x in await db.OTCs.ToListAsync())
                {
                    try
                    {
                        using (var tableResponse1 = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
                        {
                            code = "eosotcbackup",
                            scope = "eosio.token",
                            table = "order",
                            json = true,
                            limit = 65535
                        }))
                        using (var tableResponse2 = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
                        {
                            code = "eosotcbackup",
                            scope = x.Contract,
                            table = "order",
                            json = true,
                            limit = 65535
                        }))
                        {
                            var rows1 = JsonConvert.DeserializeObject<OrderTable>((await tableResponse1.Content.ReadAsStringAsync()))
                                .rows
                                .Where(y => y.bid.quantity.EndsWith(x.Id) && y.bid.contract == x.Contract)
                                .Where(y => y.ask.quantity.EndsWith("EOS") && y.ask.quantity == "eosio.token");
                            var rows2 = JsonConvert.DeserializeObject<OrderTable>((await tableResponse2.Content.ReadAsStringAsync()))
                                .rows
                                .Where(y => y.ask.quantity.EndsWith(x.Id) && y.ask.contract == x.Contract)
                                .Where(y => y.bid.quantity.EndsWith("EOS") && y.bid.contract == "eosio.token");
                            x.Transaction = rows1.Count() + rows2.Count();
                            if (rows1.Count() > 0)
                            {
                                x.PriceMin = rows1.Min(y => Convert.ToDouble(y.ask.quantity.Split(' ')[0]) / Convert.ToDouble(y.bid.quantity.Split(' ')[0]));
                                x.PriceMax = rows1.Max(y => Convert.ToDouble(y.ask.quantity.Split(' ')[0]) / Convert.ToDouble(y.bid.quantity.Split(' ')[0]));
                            }
                            await db.SaveChangesAsync();
                        }
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
