using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.AspNetCore.Localization;
using Andoromeda.Kyubey.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Andoromeda.Kyubey.Portal.Interface;
using Andoromeda.Kyubey.Portal.Services;

namespace Andoromeda.Kyubey.Portal
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out var Config);
            services.AddNodeServices(x =>
                 x.ProjectPath = "./Node"
                );
            services.AddTransient<ITokenRepository, TokenFileInfoRepository>();
            services.AddMvc();
            services.AddEntityFrameworkMySql()
                .AddDbContext<KyubeyContext>(x =>
                {
                    x.UseMySql(Config["MySQL"]);
                });
            services.AddTimedJob();
            services.AddBlobStorage()
                .AddEntityFrameworkStorage<KyubeyContext>();

            services.AddPomeloLocalization(x =>
            {
                x.AddCulture(new string[] { "en", "en-US", "en-GB" }, new JsonLocalizedStringStore(Path.Combine("Localization", "en-US.json")));
                x.AddCulture(new string[] { "zh", "zh-CN", "zh-Hans", "zh-Hans-CN", "zh-cn" }, new JsonLocalizedStringStore(Path.Combine("Localization", "zh-CN.json")));
                x.AddCulture(new string[] { "zh-Hant", "zh-Hant-TW", "zh-TW", "zh-tw" }, new JsonLocalizedStringStore(Path.Combine("Localization", "zh-TW.json")));
                x.AddCulture(new string[] { "ja", "ja-JP" }, new JsonLocalizedStringStore(Path.Combine("Localization", "ja-JP.json")));
            });

            services.AddAntiXss();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var rootFolder = System.AppContext.BaseDirectory;

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(rootFolder, @"Tokens")),
                RequestPath = new PathString("/token_assets")
            });
            app.UseBlobStorage("/js/jquery.fileupload.js");
            app.UseFrontendLocalizer("/scripts/localization.js");
            app.UseMvcWithDefaultRoute();
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<KyubeyContext>().Database.EnsureCreated();
                app.UseTimedJob();
            }
        }
    }
}
