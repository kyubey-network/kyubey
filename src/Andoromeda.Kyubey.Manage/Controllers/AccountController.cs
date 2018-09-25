using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage.Controllers
{
    public class AccountController : BaseController
    {
        [HttpGet]
        [GuestOnly]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [GuestOnly]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await SignInManager.PasswordSignInAsync(await UserManager.FindByNameAsync(username), password, true, false);
            if (!result.Succeeded)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Login Failed"];
                    x.Details = SR["Username or password is incorrect."];
                    x.StatusCode = 400;
                });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Password(string old, string @new, string confirm)
        {
            if (@new != confirm)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Change Password Failed"];
                    x.Details = SR["Confirm password is not equal to new password"];
                    x.StatusCode = 400;
                });
            }

            if (!await UserManager.CheckPasswordAsync(User.Current, old))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Change Password Failed"];
                    x.Details = SR["Old password is incorrect"];
                    x.StatusCode = 400;
                });
            }

            await UserManager.ChangePasswordAsync(User.Current, old, @new);

            return Prompt(x => 
            {
                x.Title = SR["Password changed"];
                x.Details = SR["Password has been updated successfully."];
            });
        }
    }
}
