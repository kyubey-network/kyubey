using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Bancor.Models;

namespace Andoromeda.Bancor.Controllers
{
    public class CurrencyController : Controller
    {
        [HttpGet("[controller]/{id}")]
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token));
        }

        [HttpGet("[controller]/{id}/buy")]
        public async Task<IActionResult> Buy([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token));
        }

        [HttpGet("[controller]/{id}/sell")]
        public async Task<IActionResult> Sell([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token));
        }

        [HttpGet("[controller]/{id}/k")]
        public async Task<IActionResult> k([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return View(await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token));
        }

        [HttpGet("[controller]/{id}/javascript")]
        public async Task<IActionResult> Javascript([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            return Content((await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token)).ScatterCode, "text/javascript");
        }

        [HttpGet("[controller]/{id}/icon")]
        public async Task<IActionResult> Icon([FromServices] KyubeyContext db, string id, CancellationToken token)
        {
            var bytes = (await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token)).Logo;
            if (bytes == null || bytes.Length == 0)
            {
                bytes = System.IO.File.ReadAllBytes("wwwroot/img/null.png");
            }
            return File(bytes, "image/png");
        }

        [HttpGet("[controller]/{id}/login")]
        public IActionResult Login(string id)
        {
            ViewBag.Symbol = id;
            return View();
        }

        [HttpGet("[controller]/{id}/manage")]
        public async Task<IActionResult> Manage([FromServices] KyubeyContext db, string id, string password, CancellationToken token)
        {
            var currency = await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token);
            var pwd = Convert.ToBase64String(Sha256(System.Text.Encoding.UTF8.GetBytes(password))); 
            if (pwd != currency.PasswordSha256)
            {
                return Content("Access denied.");
            }
            ViewBag.Password = password;
            return View(currency);
        }

        [HttpPost("[controller]/{id}/manage")]
        public async Task<IActionResult> Manage(
            [FromServices]KyubeyContext db, 
            string id, 
            string password, 
            string name,
            string email,
            string github,
            string webUrl,
            string description,
            string contract,
            string bancorTableName,
            string balancePath,
            string supplyPath,
            string scatterCode,
            IFormFile file,
            CancellationToken token)
        {
            var currency = await db.Currencies.SingleAsync(x => x.Id == id && x.Display, token);
            var pwd = Convert.ToBase64String(Sha256(System.Text.Encoding.UTF8.GetBytes(password)));
            if (pwd != currency.PasswordSha256)
            {
                return Content("Access denied.");
            }
            currency.Name = name;
            currency.Email = email;
            currency.GitHub = github;
            currency.WebUrl = webUrl;
            currency.Description = description;
            currency.Contract = contract;
            currency.BancorTableName = bancorTableName;
            currency.BalancePath = balancePath;
            currency.SupplyPath = supplyPath;
            currency.ScatterCode = scatterCode;
            if (file != null && file.Length > 0)
            {
                currency.Logo = await file.ReadAllBytesAsync();
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = id });
        }

        private static byte[] Sha256(byte[] bytes)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(bytes);
            }
        }
    }
}
