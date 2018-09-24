using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage.Controllers
{
    [Authorize]
    public class TokenController : BaseController
    {
        private static readonly Regex SymbolRegex = new Regex("^[A-Z]{1,16}$");

        [Route("[controller]/all")]
        public IActionResult Index(string name)
        {
            IEnumerable<Token> ret = DB.Tokens;

            if(!string.IsNullOrWhiteSpace(name))
            {
                ret.Where(x => x.Name.Contains(name));
            }

            // Only display owned tokens if the current user is not in root role.
            if (!User.IsInRole("Root"))
            {
                ret = ret.Where(x => x.UserId == User.Current.Id);
            }
            
            return PagedView(ret);
        }

        [Route("[controller]/create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/create")]
        public async Task<IActionResult> Create(string id, string name, CancellationToken cancellationToken)
        {
            if (await DB.Tokens.AnyAsync(x => x.Id == id, cancellationToken))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Create failed"];
                    x.Details = SR["The symbol has already been used, please contact to us for reporting abuse."];
                    x.StatusCode = 400;
                });
            }

            if (!SymbolRegex.IsMatch(id))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Create failed"];
                    x.Details = SR["The symbol must be upper case letters and the length should between 1 and 16"];
                    x.StatusCode = 400;
                });
            }

            var token = new Token
            {
                Id = id,
                Name = name,
                UserId = User.Current.Id
            };

            DB.Tokens.Add(token);
            await DB.SaveChangesAsync();

            return Prompt(x =>
            {
                x.Title = SR["Token created"];
                x.Details = SR["The token {0} has been created successfully.", id];
                x.RedirectText = SR["Manage Token"];
                x.RedirectUrl = Url.Action("Manage", new { id });
                x.HideBack = true;
            });
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Manage(string id, CancellationToken cancellationToken)
        {
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
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}")]
        public async Task<IActionResult> Manage(string id, string name, string contract, string webUrl, string gitHub, string email, CancellationToken cancellationToken)
        {
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

            if (name.Length <= 0 || name.Length >= 32)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Invalid name"];
                    x.Details = SR["Name length should longer than 0 char and shorter than 32 chars."];
                    x.StatusCode = 401;
                });
            }

            token.Name = name;
            token.Contract = contract;
            token.WebUrl = webUrl;
            token.GitHub = gitHub;
            token.Email = email;

            await DB.SaveChangesAsync(cancellationToken);
            return Prompt(x =>
            {
                x.Title = SR["Token updated"];
                x.Details = SR["The basic info has been updated successfully"];
            });
        }
        
        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/description")]
        public Task<IActionResult> ManageDescription(string id, CancellationToken cancellationToken)
        {
            return Manage(id, cancellationToken);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/description")]
        public async Task<IActionResult> ManageDescription(string id, string description, CancellationToken cancellationToken)
        {
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
            
            token.Description = description;

            await DB.SaveChangesAsync(cancellationToken);
            return Prompt(x =>
            {
                x.Title = SR["Token updated"];
                x.Details = SR["The token description has been updated successfully"];
            });
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/icon.png")]
        public async Task<IActionResult> Icon(string id, CancellationToken cancellationToken)
        {
            var token = await DB.Tokens.SingleAsync(x => x.Id == id, cancellationToken);
            return File(token.Icon, "image/png", "icon.png");
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/icon")]
        public Task<IActionResult> ManageIcon(string id, CancellationToken cancellationToken)
        {
            return Manage(id, cancellationToken);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/icon")]
        public async Task<IActionResult> ManageIcon(string id, IFormFile file, CancellationToken cancellationToken)
        {
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

            if (file == null || file.Length == 0)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Update token icon failed"];
                    x.Details = SR["Please upload *.png file and the size should be 150 * 150"];
                    x.StatusCode = 401;
                });
            }

            token.Icon = await file.ReadAllBytesAsync();

            await DB.SaveChangesAsync(cancellationToken);
            return Prompt(x =>
            {
                x.Title = SR["Token updated"];
                x.Details = SR["The token icon has been updated successfully"];
            });
        }

        [HttpGet]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/curve")]
        public async Task<IActionResult> ManageCurve(string id, CancellationToken cancellationToken)
        {
            ViewBag.Curves = await DB.Curves.ToListAsync(cancellationToken);
            return await Manage(id, cancellationToken);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id:regex(^[[A-Z]]{{1,16}}$)}/curve")]
        public async Task<IActionResult> ManageCurve(string id, string curveId, string arguments, CancellationToken cancellationToken)
        {
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

            if (!string.IsNullOrWhiteSpace(curveId))
            {
                if (!await DB.Curves.AnyAsync(x => x.Id == curveId))
                {
                    return Prompt(x =>
                    {
                        x.Title = SR["Token update failed"];
                        x.Details = SR["Curve not found"];
                        x.StatusCode = 400;
                    });
                }

                var curve = await DB.Curves.SingleAsync(x => x.Id == curveId, cancellationToken);
                token.CurveId = curve.Id;
                token.CurveArguments = arguments;

                if (JsonConvert.DeserializeObject<IEnumerable<double>>(arguments).Count() != curve.Arguments.Object.Count())
                {
                    return Prompt(x =>
                    {
                        x.Title = SR["Token update failed"];
                        x.Details = SR["Provided arguments do not match the curve"];
                        x.StatusCode = 400;
                    });
                }
            }
            else
            {
                token.CurveArguments = "[]";
            }
            
            await DB.SaveChangesAsync(cancellationToken);
            return Prompt(x =>
            {
                x.Title = SR["Token updated"];
                x.Details = SR["The token curve has been updated successfully"];
            });
        }
    }
}
