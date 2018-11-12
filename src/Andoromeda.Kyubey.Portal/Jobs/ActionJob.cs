using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Models;
using Andoromeda.Kyubey.Portal.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pomelo.AspNetCore.TimedJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Jobs
{
    public class ActionJob : Job
    {
        [Invoke(Begin = "2018-06-01", Interval = 1000 * 5, SkipWhileExecuting = true)]
        public void PollDexActions(IConfiguration config, KyubeyContext db)
        {
            TryHandleDexActionAsync(config, db).Wait();
        }

        [Invoke(Begin = "2018-06-01", Interval = 1000 * 15, SkipWhileExecuting = true)]
        public void PollIboActions(IConfiguration config, KyubeyContext db, ITokenRepository tokenRepository)
        {
            try
            {
                var tokens = db.Tokens
                    .Where(x => x.HasIncubation)
                    .ToList();

                foreach (var x in tokens)
                {
                    var token = tokenRepository.GetOne(x.Id);
                    if (token.Incubation == null
                        || token.Basic == null
                        || token.Basic.Contract == null
                        || string.IsNullOrEmpty(token.Basic.Contract.Transfer))
                    {
                        continue;
                    }
                    TryHandleIboActionAsync(config, db, x.Id, tokenRepository).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task TryHandleIboActionAsync(
            IConfiguration config, KyubeyContext db,
            string tokenId, ITokenRepository tokenRepository)
        {
            while (true)
            {
                var actions = await LookupIboActionAsync(config, db, tokenId, tokenRepository);
                foreach (var act in actions)
                {
                    Console.WriteLine($"Handling action log {act.account_action_seq} {act.action_trace.act.name}");
                    var blockTime = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(act.block_time + 'Z'));
                    switch (act.action_trace.act.name)
                    {
                        case "transfer":
                            var token = tokenRepository.GetOne(tokenId);
                            await HandleRaiseLogAsync(db, act.action_trace.act.data, blockTime, tokenId, token.Basic.Contract.Transfer);
                            break;
                        default:
                            continue;
                    }
                }

                if (actions.Count() < 100)
                {
                    break;
                }
            }
        }

        private async Task TryHandleDexActionAsync(IConfiguration config, KyubeyContext db)
        {
            while (true)
            {
                var actions = await LookupDexActionAsync(config, db);
                if (actions != null)
                {
                    foreach (var act in actions)
                    {
                        Console.WriteLine($"Handling action log {act.account_action_seq} {act.action_trace.act.name}");
                        var blockTime = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(act.block_time + 'Z'));
                        switch (act.action_trace.act.name)
                        {
                            case "sellmatch":
                                await HandleSellMatchAsync(db, act.action_trace.act.data, blockTime);
                                break;
                            case "buymatch":
                                await HandleBuyMatchAsync(db, act.action_trace.act.data, blockTime);
                                break;
                            case "sellreceipt":
                                await HandleSellReceiptAsync(db, act.action_trace.act.data, blockTime);
                                break;
                            case "buyreceipt":
                                await HandleBuyReceiptAsync(db, act.action_trace.act.data, blockTime);
                                break;
                            case "cancelbuy":
                                await HandleCancelBuyAsync(db, act.action_trace.act.data, blockTime);
                                break;
                            case "cancelsell":
                                await HandleCancelSellAsync(db, act.action_trace.act.data, blockTime);
                                break;
                            default:
                                continue;
                        }
                    }
                }
                if (actions == null || actions.Count() < 100)
                {
                    break;
                }
            }
        }

        private async Task HandleRaiseLogAsync(KyubeyContext db, TransferActionData data, DateTime time, string tokenId, string transferContractAccount)
        {
            try
            {
                if (data.from != transferContractAccount && data.to == transferContractAccount)
                {
                    db.RaiseLogs.Add(new RaiseLog
                    {
                        Account = data.from,
                        Amount = Convert.ToDouble(data.quantity.Split(' ')[0]),
                        Timestamp = time,
                        TokenId = tokenId
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleCancelSellAsync(KyubeyContext db, ActionDataWrap data, DateTime time)
        {
            try
            {
                var order = await db.DexSellOrders.SingleOrDefaultAsync(x => x.Id == data.id && x.TokenId == data.symbol);
                if (order != null)
                {
                    db.DexSellOrders.Remove(order);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleCancelBuyAsync(KyubeyContext db, ActionDataWrap data, DateTime time)
        {
            try
            {
                var order = await db.DexBuyOrders.SingleOrDefaultAsync(x => x.Id == data.id && x.TokenId == data.symbol);
                if (order != null)
                {
                    db.DexBuyOrders.Remove(order);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleSellReceiptAsync(KyubeyContext db, ActionDataWrap data, DateTime time)
        {
            try
            {
                var token = data.data.bid.Split(' ')[1];
                var order = await db.DexSellOrders.SingleOrDefaultAsync(x => x.Id == data.data.id && x.TokenId == token);
                if (order != null)
                {
                    db.DexSellOrders.Remove(order);
                    await db.SaveChangesAsync();
                }
                order = new DexSellOrder
                {
                    Id = data.data.id,
                    Account = data.data.account,
                    Ask = Convert.ToDouble(data.data.ask.Split(' ')[0]),
                    Bid = Convert.ToDouble(data.data.bid.Split(' ')[0]),
                    UnitPrice = data.data.unit_price / 10000.0,
                    Time = time,
                    TokenId = token
                };
                db.DexSellOrders.Add(order);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleBuyReceiptAsync(KyubeyContext db, ActionDataWrap data, DateTime time)
        {
            try
            {
                var token = data.data.ask.Split(' ')[1];
                var order = await db.DexBuyOrders.SingleOrDefaultAsync(x => x.Id == data.data.id && x.TokenId == token);
                if (order != null)
                {
                    db.DexBuyOrders.Remove(order);
                    await db.SaveChangesAsync();
                }
                order = new DexBuyOrder
                {
                    Id = data.data.id,
                    Account = data.data.account,
                    Ask = Convert.ToDouble(data.data.ask.Split(' ')[0]),
                    Bid = Convert.ToDouble(data.data.bid.Split(' ')[0]),
                    UnitPrice = data.data.unit_price / 10000.0,
                    Time = time,
                    TokenId = token
                };
                db.DexBuyOrders.Add(order);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleSellMatchAsync(KyubeyContext db, ActionDataWrap data, DateTime time)
        {
            try
            {
                var token = data.data.bid.Split(' ')[1];
                var bid = Convert.ToDouble(data.data.bid.Split(' ')[0]);
                var ask = Convert.ToDouble(data.data.ask.Split(' ')[0]);
                var order = await db.DexBuyOrders.SingleOrDefaultAsync(x => x.Id == data.data.id && x.TokenId == token);
                if (order != null)
                {
                    order.Bid -= bid;
                    order.Ask -= ask;
                    if (order.Ask <= 0 || order.Bid <= 0)
                    {
                        db.DexBuyOrders.Remove(order);
                    }
                    await db.SaveChangesAsync();
                }
                db.MatchReceipts.Add(new MatchReceipt
                {
                    Ask = ask,
                    Bid = bid,
                    Asker = data.data.asker,
                    Bidder = data.data.bidder,
                    Time = time,
                    TokenId = token,
                    UnitPrice = data.data.unit_price / 10000.0
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleBuyMatchAsync(KyubeyContext db, ActionDataWrap data, DateTime time)
        {
            try
            {
                var token = data.data.ask.Split(' ')[1];
                var bid = Convert.ToDouble(data.data.bid.Split(' ')[0]);
                var ask = Convert.ToDouble(data.data.ask.Split(' ')[0]);
                var order = await db.DexSellOrders.SingleOrDefaultAsync(x => x.Id == data.data.id && x.TokenId == token);
                if (order != null)
                {
                    order.Bid -= bid;
                    order.Ask -= ask;
                    if (order.Ask <= 0 || order.Bid <= 0)
                    {
                        db.DexSellOrders.Remove(order);
                    }
                    await db.SaveChangesAsync();
                }
                db.MatchReceipts.Add(new MatchReceipt
                {
                    Ask = ask,
                    Bid = bid,
                    Asker = data.data.asker,
                    Bidder = data.data.bidder,
                    Time = time,
                    TokenId = token,
                    UnitPrice = data.data.unit_price / 10000.0
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task<IEnumerable<EosAction<ActionDataWrap>>> LookupDexActionAsync(IConfiguration config, KyubeyContext db)
        {
            try
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
                    var result = JsonConvert.DeserializeObject<EosActionWrap<ActionDataWrap>>(txt, new JsonSerializerSettings
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task<IEnumerable<EosAction<TransferActionData>>> LookupIboActionAsync(
            IConfiguration config, KyubeyContext db,
            string tokenId, ITokenRepository tokenRepository)
        {
            var tokenInDb = await db.Tokens.SingleAsync(x => x.Id == tokenId);
            var position = tokenInDb.ActionPosition;
            var token = tokenRepository.GetOne(tokenId);
            using (var client = new HttpClient { BaseAddress = new Uri(config["TransactionNodeBackup"]) })
            using (var response = await client.PostAsJsonAsync("/v1/history/get_actions", new
            {
                account_name = token.Basic.Contract.Transfer,
                pos = position,
                offset = 100
            }))
            {
                var txt = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<EosActionWrap<TransferActionData>>(txt, new JsonSerializerSettings
                {
                    Error = HandleDeserializationError
                });
                if (result.actions.Count() == 0)
                {
                    return null;
                }
                if (result.actions.Count() > 0)
                {
                    tokenInDb.ActionPosition = result.actions.Last().account_action_seq;
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
