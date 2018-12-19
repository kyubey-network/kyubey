using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.Portal.Interface;
using Andoromeda.Kyubey.Portal.Models;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Services
{
    public class TokenAPIRopository : ITokenAPIRopository
    {
        private readonly INodeServices _node;
        private readonly KyubeyContext _db;
        private readonly ITokenRepository _tokenRepository;
        private readonly IConfiguration _config;

        public TokenAPIRopository(INodeServices node, KyubeyContext db, ITokenRepository tokenRepository, IConfiguration config)
        {
            _node = node;
            _db = db;
            _tokenRepository = tokenRepository;
            _config = config;
        }
        public async Task<TokenContractPriceModel> GetTokenContractPriceAsync(string id)
        {
            try
            {
                using (var txClient = new HttpClient { BaseAddress = new Uri(_config["TransactionNode"]) })
                {
                    var currentTokenInfo = _tokenRepository.GetOne(id);
                    var currentPriceJavascript = _tokenRepository.GetPriceJsText(id);
                    using (var tableResponse = await txClient.PostAsJsonAsync("/v1/chain/get_table_rows", new
                    {
                        code = currentTokenInfo.Basic.Contract.Pricing,
                        scope = currentTokenInfo.Basic.Price_Scope,
                        table = currentTokenInfo.Basic.Price_Table,
                        json = true
                    }))
                    {
                        if (string.IsNullOrWhiteSpace(currentTokenInfo.Basic.Price_Scope) || string.IsNullOrWhiteSpace(currentTokenInfo.Basic.Price_Scope))
                        {
                            return new TokenContractPriceModel
                            {
                                BuyPrice = 0,
                                SellPrice = 0
                            };
                        }

                        var text = await tableResponse.Content.ReadAsStringAsync();
                        var rows = JsonConvert.DeserializeObject<Table>(text).rows;

                        var buy = await _node.InvokeExportAsync<string>("./price", "buyPrice", rows, currentPriceJavascript);
                        var sell = await _node.InvokeExportAsync<string>("./price", "sellPrice", rows, currentPriceJavascript);
                        var buyPrice = Convert.ToDecimal(buy.Contains(".") ? buy.TrimEnd('0') : buy);
                        var sellPrice = Convert.ToDecimal(sell.Contains(".") ? sell.TrimEnd('0') : sell);
                        return new TokenContractPriceModel
                        {
                            BuyPrice = buyPrice,
                            SellPrice = sellPrice
                        };
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new TokenContractPriceModel
                {
                    BuyPrice = 0,
                    SellPrice = 0
                };
            }
        }
    }
}
