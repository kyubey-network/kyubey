using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage.Controllers
{
    public class DexController : BaseController
    {
        [Route("[controller]/all")]
        public IActionResult Index(string name)
        {
            IEnumerable<Dex> ret = DB.Dexes.Include(x => x.Token);

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
            if (await DB.Dexes.AnyAsync(x => x.Id == id, cancellationToken))
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
            if (await DB.Dexes.AnyAsync(x => x.Id == id, cancellationToken))
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

            var dex = new Dex
            {
                Id = id,
                Status = Status.Reviewing
            };

            DB.Dexes.Add(dex);
            await DB.SaveChangesAsync();
            return Prompt(x =>
            {
                x.Title = SR["Onboarded"];
                x.Details = SR["The token has onboarded dex exchange"];
                x.HideBack = true;
                x.RedirectText = SR["Configure"];
                x.RedirectUrl = Url.Action("Manage", new { id = dex.Id });
            });
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Manage(string id, CancellationToken cancellationToken)
        {
            var dex = await DB.Dexes
                .Include(x => x.Token)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (dex == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token <{0}> has not been found in dex exchange.", id];
                    x.StatusCode = 404;
                });
            }
            
            if (!User.IsInRole("ROOT") && User.Current.Id != dex.Token.UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Permission Denied"];
                    x.Details = SR["You don't have the access to {0}", dex.Token.Id];
                    x.StatusCode = 401;
                });
            }

            return View(dex);
        }

        [HttpGet]
        [Authorize(Roles = "ROOT")]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/status")]
        public async Task<IActionResult> ManageStatus(string id, CancellationToken cancellationToken)
        {
            return await Manage(id, cancellationToken);
        }

        [HttpPost]
        [Authorize(Roles = "ROOT")]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/status")]
        public async Task<IActionResult> ManageStatus(string id, Status status, CancellationToken cancellationToken)
        {
            var dex = await DB.Dexes
                .Include(x => x.Token)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (dex == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Token not found"];
                    x.Details = SR["The token <{0}> has not been found in bancor exchange.", id];
                    x.StatusCode = 404;
                });
            }

            if (!User.IsInRole("ROOT"))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Permission Denied"];
                    x.Details = SR["You don't have the access to {0}", dex.Token.Id];
                    x.StatusCode = 401;
                });
            }

            dex.Status = status;
            await DB.SaveChangesAsync();

            return Prompt(x =>
            {
                x.Title = SR["DEX info updated"];
                x.Details = SR["The DEX status has been updated successfully"];
            });
        }
    }
}
