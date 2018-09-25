using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage.Controllers
{
    public class BancorController : BaseController
    {
        [Route("[controller]/all")]
        public IActionResult Index(string name)
        {
            IEnumerable<Bancor> ret = DB.Bancors.Include(x => x.Token);

            if (!string.IsNullOrWhiteSpace(name))
            {
                ret.Where(x => x.Token.Name.Contains(name) || x.Id.Contains(name));
            }

            // Only display owned tokens if the current user is not in root role.
            if (!User.IsInRole("Root"))
            {
                ret = ret.Where(x => x.Token.UserId == User.Current.Id);
            }

            return PagedView(ret);
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/onboard")]
        public async Task<IActionResult> Onboard(string id, CancellationToken cancellationToken)
        {
            if (await DB.Bancors.AnyAsync(x => x.Id == id, cancellationToken))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Cannot onboard"];
                    x.Details = SR["The token <{0}> is already onboarded", id];
                    x.StatusCode = 400;
                });
            }

            var token = await DB.Tokens.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (token == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token <{0}> has not been found in system.", id];
                    x.StatusCode = 404;
                });
            }

            if (!User.IsInRole("ROOT") && User.Current.Id != token.UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Permission Denied"];
                    x.Details = SR["You don't have the access to {0}", token.Id];
                    x.StatusCode = 401;
                });
            }

            return View(token);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/onboard")]
        public async Task<IActionResult> Onboard(string id, string _, CancellationToken cancellationToken)
        {
            if (await DB.Bancors.AnyAsync(x => x.Id == id, cancellationToken))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Cannot onboard"];
                    x.Details = SR["The token <{0}> is already onboarded", id];
                    x.StatusCode = 400;
                });
            }

            var token = await DB.Tokens.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (token == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token <{0}> has not been found in system.", id];
                    x.StatusCode = 404;
                });
            }

            if (!User.IsInRole("ROOT") && User.Current.Id != token.UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Permission Denied"];
                    x.Details = SR["You don't have the access to {0}", token.Id];
                    x.StatusCode = 401;
                });
            }

            var bancor = new Bancor
            {
                Id = id,
                Status = Status.Preparing,
                TradeJavascript = @"function buy() {
    var contract_account = 'your_contract_account';
    var amount = $('#amount').val();
    // You can use the variable: account, requiredFields in this script
    eos.contract('eosio.token', { requiredFields }).then(contract => {
        return contract.transfer(
            account.name, 
            contract_account, 
            $('#amount').val().toFixed(4) + ' EOS', 
            ``, 
            { 
                authorization: [`${account.name}@${account.authority}`] 
            });
    })
}

function sell() {
    eos.contract(contract_account, { requiredFields }).then(contract => {
        return contract.transfer(
            account.name, 
            contract_account, 
            $('#amount').val().toFixed(4) + ' <YOUR SYMBOL>', 
            ``, 
            { 
                authorization: [`${account.name}@${account.authority}`] 
            });
    })
}",
                CurrentBuyPriceJavascript = @"function getCurrentBuyPrice(rows) {
    // The rows are from the bancor table you specified
    return '0.0000 EOS';
}",
                CurrentSellPriceJavascript = @"function getCurrentSellPrice(rows) {
    // The rows are from the bancor table you specified
    return '0.0000 EOS';
}"
            };

            DB.Bancors.Add(bancor);
            await DB.SaveChangesAsync();
            return Prompt(x =>
            {
                x.Title = SR["Onboarded"];
                x.Details = SR["The token has onboarded bancor exchange"];
                x.HideBack = true;
                x.RedirectText = SR["Configure"];
                x.RedirectUrl = Url.Action("Manage", new { id = bancor.Id });
            });
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Manage(string id, CancellationToken cancellationToken)
        {
            var bancor = await DB.Bancors
                .Include(x => x.Token)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (bancor == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token <{0}> has not been found in bancor exchange.", id];
                    x.StatusCode = 404;
                });
            }
            
            if (!User.IsInRole("ROOT") && User.Current.Id != bancor.Token.UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Permission Denied"];
                    x.Details = SR["You don't have the access to {0}", bancor.Token.Id];
                    x.StatusCode = 401;
                });
            }

            return View(bancor);
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/price")]
        public async Task<IActionResult> ManagePrice(string id, CancellationToken cancellationToken)
        {
            return await Manage(id, cancellationToken);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Manage(string id, string tradeJavascript, CancellationToken cancellationToken)
        {
            var bancor = await DB.Bancors
                .Include(x => x.Token)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (bancor == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token <{0}> has not been found in bancor exchange.", id];
                    x.StatusCode = 404;
                });
            }

            if (!User.IsInRole("ROOT") && User.Current.Id != bancor.Token.UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Permission Denied"];
                    x.Details = SR["You don't have the access to {0}", bancor.Token.Id];
                    x.StatusCode = 401;
                });
            }

            bancor.TradeJavascript = tradeJavascript;
            if (!User.IsInRole("ROOT"))
            {
                bancor.Status = Status.Reviewing;
            }
            await DB.SaveChangesAsync();

            return Prompt(x => 
            {
                x.Title = SR["Bancor info updated"];
                x.Details = SR["The bancor basic info has been updated successfully"];
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/price")]
        public async Task<IActionResult> ManagePrice(
            string id, 
            string table, 
            string scope, 
            string buyPriceJavascript,
            string sellPriceJavascript,
            CancellationToken cancellationToken)
        {
            var bancor = await DB.Bancors
                .Include(x => x.Token)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (bancor == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token <{0}> has not been found in bancor exchange.", id];
                    x.StatusCode = 404;
                });
            }

            if (!User.IsInRole("ROOT") && User.Current.Id != bancor.Token.UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Permission Denied"];
                    x.Details = SR["You don't have the access to {0}", bancor.Token.Id];
                    x.StatusCode = 401;
                });
            }

            bancor.Table = table;
            bancor.Scope = scope;
            bancor.CurrentBuyPriceJavascript = buyPriceJavascript;
            bancor.CurrentSellPriceJavascript = sellPriceJavascript;
            await DB.SaveChangesAsync();

            return Prompt(x =>
            {
                x.Title = SR["Bancor info updated"];
                x.Details = SR["The bancor price info has been updated successfully"];
            });
        }
    }
}
