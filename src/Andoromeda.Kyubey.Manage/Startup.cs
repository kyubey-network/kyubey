using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Pomelo.AspNetCore.Localization;
using Andoromeda.Kyubey.Models;

namespace Andoromeda.Kyubey.Manage
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out var Configuration);
            services.AddMvc();
            services.AddNodeServices(x =>
            {
                x.ProjectPath = "./Node";
            });
            services.AddDbContext<KyubeyContext>(x =>
            {
                x.UseMySql(Configuration["MySQL"]);
            });
            services.AddMemoryCache();

            services.AddIdentity<User, UserRole>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddEntityFrameworkStores<KyubeyContext>()
                .AddDefaultTokenProviders();

            services.AddBlobStorage()
                .AddEntityFrameworkStorage<KyubeyContext>();

            services.AddPomeloLocalization(x =>
            {
                x.AddCulture(new string[] { "zh", "zh-CN", "zh-Hans", "zh-Hans-CN", "zh-cn" }, new JsonLocalizedStringStore(Path.Combine("Localization", "zh-CN.json")));
                x.AddCulture(new string[] { "en", "en-US", "en-GB" }, new JsonLocalizedStringStore(Path.Combine("Localization", "en-US.json")));
                x.AddCulture(new string[] { "ja", "ja-JP" }, new JsonLocalizedStringStore(Path.Combine("Localization", "ja-JP.json")));
            });

            services.AddMvc();
            services.AddSmartCookies();
            services.AddSmartUser<User, long>();
            services.AddAntiXss();
            services.AddTimedJob();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseBlobStorage("/scripts/jquery.fileupload.js");
            app.UseFrontendLocalizer("/scripts/localization.js");
            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<KyubeyContext>().InitializeDatabaseAsync(
                    serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>(),
                    serviceScope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>()).Wait();

                app.UseTimedJob();
            }
        }
    }
}
