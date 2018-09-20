using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Bancor.Models;

namespace Andoromeda.Bancor.Controllers
{
    public class HomeController : BaseController
    {
        // GET: /<controller>/
        public async Task<IActionResult> Index([FromServices] KyubeyContext db, CancellationToken token)
        {
            var currencies = await db.Currencies
                .Where(x => x.Display)
                .OrderByDescending(x => x.PRI)
                .ToListAsync(token);
            return View(currencies);
        }

        public async Task<IActionResult> OTC([FromServices] KyubeyContext db, CancellationToken token)
        {
            var currencies = await db.OTCs
                .ToListAsync(token);
            return View(currencies);
        }

        [HttpGet]
        public IActionResult OnBoard()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OnBoard([FromServices] KyubeyContext db, Currency model, string password, IFormFile file)
        {
            //model.Display = true;
            model.PasswordSha256 = Convert.ToBase64String(Sha256(System.Text.Encoding.UTF8.GetBytes(password)));
            if (file != null && file.Length > 0)
            {
                model.Logo = await file.ReadAllBytesAsync();
            }
            db.Currencies.Add(model);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Currency", new { id = model.Id });
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
